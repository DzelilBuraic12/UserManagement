using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.SymbolStore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Data;
using UserManagement.Domain.Entities;
using UserManagement.DTOs;

namespace UserManagement.Services
{
    using DomainUser = UserManagement.Domain.Entities.User;

    public class AuthService :IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<DomainUser> _passwordHasher;
        private readonly IConfiguration _config;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext db,
            IPasswordHasher<DomainUser> passwordHasher,
            IConfiguration config,
            IValidator<LoginDto> loginValidator,
            IValidator<RegisterDto> registerValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<ResetPasswordDto> resetPasswordValidator,
            IMapper mapper)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _config = config;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _changePasswordValidator = changePasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _mapper = mapper;
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();

            if (input.Length == 1)
                return input.ToUpperInvariant();

            return char.ToUpperInvariant(input[0]) + input.Substring(1).ToLowerInvariant();
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {

            var validation = await _loginValidator.ValidateAsync(loginDto);

            if(!validation.IsValid)
            {
                throw new ValidationException(validation.Errors);
            }
            var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.IsActive == false)
            {
                throw new UnauthorizedAccessException("Account is deactivated.");

            }

            var passwordHash = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (passwordHash != PasswordVerificationResult.Success)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Secret"])
                );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"])),
                signingCredentials: creds
                );

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"])),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _db.RefreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();

            var result = new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]))
            };

            return result;
        }

        public async Task<AuthResultDto> RefreshAsync(string refreshToken)
        {
            
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token is requierd.");
            }

            var token = await _db.RefreshTokens.Include(rt => rt.User)
                .Where(rt => rt.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (token == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if(token.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }
            if (token.IsRevoked)
            {
                throw new UnauthorizedAccessException("Refresh token revoked.");
            }

            if (token.User == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            if (token.User.IsActive == false)
            {
                throw new UnauthorizedAccessException("Account is deactivated.");
            }

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, token.User.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, token.User.Email),
                new Claim(ClaimTypes.Role, token.User.Role)
            };

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Secret"])
                );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newToken = new JwtSecurityToken
            (
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"])),
                signingCredentials: creds
             );

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(newToken);

            var newRefreshToken = Guid.NewGuid().ToString();

            token.IsRevoked = true;
            var refreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = token.User.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"])),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            await _db.RefreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();

            var result = new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]))
            };

            return result;

        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            var validation = await _registerValidator.ValidateAsync(dto);

            if(!validation.IsValid)
            {
                throw new ValidationException(validation.Errors);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var exist = await _db.Users.AnyAsync(u => u.Email == normalizedEmail);

            if(exist)
            {
                throw new ValidationException("Email already exists.");
            }

            var user = _mapper.Map<DomainUser>(dto);
            user.Email = normalizedEmail;

            user.FirstName = CapitalizeFirstLetter(dto.FirstName);
            user.LastName = CapitalizeFirstLetter( dto.LastName);

            user.Role = Domain.Constants.Roles.User;
            user.IsActive = true;

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var validation = await _changePasswordValidator.ValidateAsync(dto);

            if(!validation.IsValid)
            {
                throw new ValidationException(validation.Errors);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null)
            {
                return false;
            }

            var result =  _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);

            if(result != PasswordVerificationResult.Success)
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            if(dto.NewPassword == dto.CurrentPassword)
            {
                throw new ValidationException("New password must be different from old password.");
            }

            var newHash = _passwordHasher.HashPassword(user, dto.NewPassword);

            user.PasswordHash = newHash;

            await _db.SaveChangesAsync();

            return true;

        }

        public async Task<bool> ResetPasswordStartAsync(string email)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                return false;
            }

            if(!user.IsActive)
            {
                return false;
            }

            var resetToken = Guid.NewGuid().ToString();

            var oldTokens = await _db.PasswordResetTokens.Where(p => p.UserId == user.Id && !p.IsUsed && p.ExpiresAt > DateTime.UtcNow).ToListAsync();

            foreach(var oldToken in oldTokens)
            {
                oldToken.IsUsed = true;
            }

            var resetTokenEntity = new PasswordResetToken
            {
                Token = resetToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _db.PasswordResetTokens.AddAsync(resetTokenEntity);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordCompleteAsync(ResetPasswordDto dto)
        {
            var validation = await _resetPasswordValidator.ValidateAsync(dto);

            if(!validation.IsValid)
            {
                throw new ValidationException(validation.Errors);
            }

            var resetToken = await _db.PasswordResetTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == dto.Token);

            if(resetToken == null)
            {
                return false;
            }

            if(resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            if (resetToken.IsUsed)
            {
                return false;
            }

            var user = resetToken.User;

            if(user == null)
            {
                return false;
            }

            if(!user.IsActive)
            {
                return false;
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            resetToken.IsUsed = true;

            await _db.SaveChangesAsync();
            return true;

        }


    }
}
