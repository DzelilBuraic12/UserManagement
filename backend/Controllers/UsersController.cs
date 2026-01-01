using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Services;
using FluentValidation;
using System.Security.Claims;
using UserManagement.Contracts;
using UserManagement.Domain.Constants;
namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        //GET /api/users/current
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentAsync()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var user = await _userService.GetCurrentAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        //GET /api/users/all

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        //GET /api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] UserQuery query)
        {
            try
            {
                var result = await _userService.ListAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during get all users.", error = ex.Message });
            }
        }

        //GET /api/users/technicians
        [HttpGet("technicians")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTechniciansAsync()
        {
            var techs = await _userService.GetTechniciansAsync();
            return Ok(techs);
        }


        //GET /api/users/id
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured during get user id.", error = ex.Message });
            }
        }

        //POST /api/users
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            try
            {
                var userId = await _userService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = userId }, new { id = userId, message = "User created successfully." });

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured during create user.", error = ex.Message });
            }
        }


        //PUT /api/users/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            try
            {
                int performedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _userService.UpdateAsync(id, dto, performedByUserId);

                if (result)
                {
                    return Ok(new { message = "User update successfully." });
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
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during update.", error = ex.Message });
            }
        }

        //POST /api/users/{id}/assign-role
        [HttpPost("{id}/assign-role")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AssignRole(int id, AssignRoleDto dto)
        {
            try
            {
                var performedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _userService.AssignRoleAsync(id, dto.Role, performedByUserId);
                if (result)
                {
                    return Ok(new { message = "Role assigned successfully." });
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during assign.", error = ex.Message });
            }
        }

        //POST /api/users/{id}/deactivate
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var performedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _userService.DeactivateAsync(id, performedByUserId);

                if (result)
                {
                    return Ok(new { message = "User deactivated successfully." });
                }
                else
                {
                    return NotFound(new { message = "User not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during deactivate.", error = ex.Message });
            }
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var performedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _userService.ActivateAsync(id, performedByUserId);

                if (result)
                    return Ok(new { message = "User activated successfully." });
                else
                    return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred durring activate.", error = ex.Message });
            }
        }
    }
}
