using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Services;
using FluentValidation;
using System.Security.Claims;


namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //POST /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
        {
            try
            {
                await _authService.RegisterAsync(dto);
                return Ok(new { message = "Registration successful. You can now log in." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred durig registration.", error = ex.Message });
            }
        }

        //POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new {errors = ex.Errors.Select(e => e.ErrorMessage)});
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login." , error = ex.Message});
            }
        }

        //POST /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshAsync(request.RefreshToken);
                return Ok(result);
            }
            catch(ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during token refresh.", error = ex.Message});
            }
        }

        //POST /api/auth/changepassword

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _authService.ChangePasswordAsync(userId, dto);
                
                if(result)
                {
                    return Ok(new { message = "Password changed successfully." });
                }
                else
                {
                    return NotFound(new { message = "User not found." });
                }
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during change password.", error = ex.Message });
            }
        }

        //POST /api/auth/reset-password/start
        [HttpPost("reset-password/start")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordStartAsync([FromBody] ResetPasswordStartRequest request )
        {
            try
            {
                var result = await _authService.ResetPasswordStartAsync(request.Email);
                return Ok(new { message = "If the email exists, a password reset link has been sent." });

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during reset password.", error = ex.Message });
            }
        }

        //POST /api/auth/reset-password/complete
        [HttpPost("reset-password/complete")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordCompleteAsync([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var result = await _authService.ResetPasswordCompleteAsync(dto);
                if (result)
                {
                    return Ok(new { message = "Password has been reset successfully. You can now log in." });
                }
                else
                {
                    return BadRequest(new { message = "Invalid or expired reset token." });
                }
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage)});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during reset password complete.", error = ex.Message });
            }

        }
    }
}
