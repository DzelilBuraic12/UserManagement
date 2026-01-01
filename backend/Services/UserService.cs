using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Common;
using UserManagement.Contracts;
using UserManagement.Data;
using UserManagement.Domain.Entities;
using UserManagement.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System;


namespace UserManagement.Services
{
    using DomainUser = UserManagement.Domain.Entities.User;

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IValidator<UserCreateDto> _createValidator;
        private readonly IValidator<UserUpdateDto> _updateValidator;
        private readonly IPasswordHasher<DomainUser> _passwordHasher;


        public UserService(AppDbContext db,
            IMapper mapper, IValidator<UserCreateDto> createValidator,
            IValidator<UserUpdateDto> updateValidator,
            IPasswordHasher<DomainUser> passwordHasher)
        {
            _db = db;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _passwordHasher = passwordHasher;
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
        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return Domain.Constants.Roles.User;

            var normalized = role.Trim();

            if (normalized.Equals("admin", StringComparison.OrdinalIgnoreCase))
                return Domain.Constants.Roles.Admin;

            if (normalized.Equals("technician", StringComparison.OrdinalIgnoreCase))
                return Domain.Constants.Roles.Technician;

            if (normalized.Equals("user", StringComparison.OrdinalIgnoreCase))
                return Domain.Constants.Roles.User;

            throw new ValidationException($"Invalid role: {role}. Allowed values: Admin, Technician, User");
        }


        public async Task<int> CreateAsync(UserCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var exist = await _db.Users.AnyAsync(u => u.Email == normalizedEmail);

            if (exist)
            {
                throw new ValidationException("Email already exist!");
            }

            var user = _mapper.Map<DomainUser>(dto);
            user.Email = normalizedEmail;
            user.FirstName = CapitalizeFirstLetter(dto.FirstName);
            user.LastName = CapitalizeFirstLetter(dto.LastName);

            user.Role = NormalizeRole(dto.Role);
            user.IsActive = true;

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _db.Users.AddAsync(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ValidationException("Email already exist!");
            }

            return user.Id;
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto, int performedByUserId)
        {
            var admin = Domain.Constants.Roles.Admin;

            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var performer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == performedByUserId);

            if (performer == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (performedByUserId != id && !string.Equals(performer.Role, admin, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var current = user.Email.Trim().ToLowerInvariant();
                var next = dto.Email.Trim().ToLowerInvariant();

                if (next != current)
                {
                    var exist = await _db.Users.AnyAsync(u => u.Email == next && u.Id != id);

                    if (exist)
                    {
                        throw new InvalidOperationException("Email already in use");

                    }

                    user.Email = next;
                }
            }

            if (dto.FirstName != null)
            {
                user.FirstName = CapitalizeFirstLetter(dto.FirstName);
            }

            if (dto.LastName != null)
            {
                user.LastName = CapitalizeFirstLetter(dto.LastName);
            }

            if (dto.IsActive != null && string.Equals(performer.Role, admin, StringComparison.OrdinalIgnoreCase))
            {
                user.IsActive = dto.IsActive.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Email already in use");
            }

        }

        public async Task<bool> AssignRoleAsync(int id, string role, int performedByUserId)
        {

            var admin = Domain.Constants.Roles.Admin;
            var technician = Domain.Constants.Roles.Technician;
            var user = Domain.Constants.Roles.User;


            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ValidationException("Role cannot be empty!");
            }

            var normalized = role.Trim();

            string targetRole;

            if (normalized.Equals(admin, StringComparison.OrdinalIgnoreCase))
            {
                targetRole = admin;
            }
            else if (normalized.Equals(technician, StringComparison.OrdinalIgnoreCase))
            {
                targetRole = technician;
            }
            else if (normalized.Equals(user, StringComparison.OrdinalIgnoreCase))
            {
                targetRole = user;
            }
            else
            {
                throw new ValidationException("Invalid role.");
            }

            var performer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == performedByUserId);

            if (performer == null || !string.Equals(performer.Role, admin, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException();
            }


            var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (target == null)
                return false;

            if (string.Equals(target.Role, targetRole, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(target.Role, admin, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(targetRole, admin, StringComparison.OrdinalIgnoreCase))
            {
                var anotherAdminExist = await _db.Users.AsNoTracking().AnyAsync(u => u.Role == admin && u.Id != target.Id);

                if (!anotherAdminExist)
                {
                    throw new InvalidOperationException("Cannot remove the last admin.");
                }
            }


            target.Role = targetRole;
            target.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new ValidationException("Unable to assign role.");
            }
        }


        public async Task<List<UserListDto>> GetTechniciansAsync()
        {
            var technician = Domain.Constants.Roles.Technician;
            var query = _db.Users.Where(u => u.Role == technician && u.IsActive);
            var users = await query.Select(u => new UserListDto { Id = u.Id, FirstName = u.FirstName, LastName = u.LastName })
                .ToListAsync();

            return users;
        }
        public async Task<List<UserReadDto>> GetAllUsersAsync()
        {
            var users = await _db.Users.Select(u => new UserReadDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
            }).ToListAsync();

            return users;
        }
        public async Task<UserReadDto?> GetCurrentAsync(int userId)
        {
            var request = await _db.Users.AsNoTracking().Where(u => u.Id == userId).
                Select(u => new UserReadDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                }).FirstOrDefaultAsync();

            return request;
        }


        public async Task<UserReadDto?> GetByIdAsync(int id)
        {
            var user = await _db.Users
                .Include(u => u.CreatedRequests)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }

            var dto = _mapper.Map<UserReadDto>(user);
            return dto;
        }

        public async Task<PagedResult<UserListDto>> ListAsync(UserQuery query)
        {
            var page = query.Page ?? 1;
            var pageSize = Math.Clamp(query.PageSize ?? 20, 1, 50);

            var user = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search;
                user = user.Where(u =>
                    u.FirstName.StartsWith(term) ||
                    u.LastName.StartsWith(term) ||
                    u.Email.StartsWith(term));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                user = user.Where(u => u.Role == query.Role);
            }
            if (query.IsActive.HasValue)
            {
                user = user.Where(u => u.IsActive == query.IsActive.Value);
            }


            var sortBy = query.SortBy?.ToLower() ?? "id";
            var sortDir = query.SortDir?.ToLower() ?? "asc";

            if (sortDir == "desc")
            {
                user = sortBy switch
                {
                    "firstname" => user.OrderByDescending(u => u.FirstName).ThenBy(u => u.Id),
                    "lastname" => user.OrderByDescending(u => u.LastName).ThenBy(u => u.Id),
                    "email" => user.OrderByDescending(u => u.Email).ThenBy(u => u.Id),
                    "createdat" => user.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id),
                    _ => user.OrderByDescending(u => u.Id)
                };
            }
            else
            {
                user = sortBy switch
                {
                    "firstname" => user.OrderBy(u => u.FirstName).ThenBy(u => u.Id),
                    "lastname" => user.OrderBy(u => u.LastName).ThenBy(u => u.Id),
                    "email" => user.OrderBy(u => u.Email).ThenBy(u => u.Id),
                    "createdat" => user.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
                    _ => user.OrderBy(u => u.Id)
                };
            }
            var total = await user.CountAsync();

            var skip = (page - 1) * pageSize;
            user = user.Skip(skip).Take(pageSize);

            var items = await user.ProjectTo<UserListDto>(_mapper.ConfigurationProvider).ToListAsync();

            return new PagedResult<UserListDto>
            {
                Data = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeactivateAsync(int id, int performedByUserId)
        {
            var admin = Domain.Constants.Roles.Admin;

            var performer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == performedByUserId);

            if (performer == null || !string.Equals(performer.Role, admin, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException();
            }


            var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (target == null)
            {
                return false;
            }

            if (target.IsActive == false)
            {
                return true;
            }

            if (string.Equals(target.Role, admin, StringComparison.OrdinalIgnoreCase))
            {
                var anotherAdminExist = await _db.Users.AsNoTracking().AnyAsync(u => u.Role == admin && u.IsActive == true && u.Id != target.Id);

                if (!anotherAdminExist)
                {
                    throw new InvalidOperationException("Cannot deactivate last admin!");
                }
            }

            target.IsActive = false;
            target.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateAsync(int id, int performedByUserId)
        {
            var admin = Domain.Constants.Roles.Admin;

            var performer = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == performedByUserId);

            if (performer == null || !string.Equals(performer.Role, admin, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException();
            }

            var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (target == null)
                return false;

            if (target.IsActive)
                return true;

            target.IsActive = true;
            target.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

    }
}
