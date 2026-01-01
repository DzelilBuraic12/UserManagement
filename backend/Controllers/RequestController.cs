using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using UserManagement.Common;
using UserManagement.Contracts;
using UserManagement.DTOs;
using UserManagement.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        //GET /api/users/requests
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List([FromQuery] RequestQuery query)
        {
            try
            {
                var result = await _requestService.ListAsync(query);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading requests.", error = ex.Message });
            }
        }

        //GET /api/requests/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _requestService.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = "Request not found." });
                }
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading request ID.", error = ex.Message });
            }
        }

        //POST /api/requests/{id}
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] RequestCreateDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _requestService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating request.", error = ex.Message });
            }

        }

        //PUT /api/requests/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] RequestUpdateDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _requestService.UpdateAsync(id, dto, userId);

                if (result == -1)
                    return NotFound(new { message = "Request not found." });

                if (result == 0)
                    return BadRequest(new { message = "Update not allowed." });

                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during update request.", error = ex.Message });
            }
        }


        //POST /api/requests/{id}/assign-technician
        [HttpPost("{id}/assign-technician")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTechnician(int id, [FromBody] int technicianId)
        {
            try
            {
                int performedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool success = await _requestService.AssignTechnicianAsync(id, technicianId, performedByUserId);

                if (success)
                {
                    return Ok(new { message = "Techinican assigned successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Cannot assign technician." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error assigning technician.", error = ex.Message });
            }
        }

        //POST /api/requests/{id}/change-status
        [HttpPost("{id}/change-status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] int newStatusId)
        {
            try
            {
                int performedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool success = await _requestService.ChangeStatusAsync(id, newStatusId, performedByUserId);

                if (success)
                {
                    return Ok(new { message = "Status changed successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Cannot change status." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error status change.", error = ex.Message });
            }
        }

        //GET /api/requests/summary
        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetSummary()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            var sumary = await _requestService.GetSummaryAsync(userId, role);
            return Ok((sumary));
        }

        //GET /api/requests/summary-with-trends
        [HttpGet("summary-with-trends")]
        [Authorize]
        public async Task<IActionResult> GetDashboardSummaryWithTrends()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            var summary = await _requestService.GetDashboardSummaryWithTrendsAsync(userId, role);
            return Ok(summary);
        }

        //GET /api/reqeuests/recent-activity
        [HttpGet("recent-activity")]
        [Authorize]
        public async Task<IActionResult> GetRecentActivityAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            var activities = await _requestService.GetRecentActivityAsync(userId, role);
            return Ok(activities);

        }

        //GET /api/requests/high-priority
        [HttpGet("high-priority")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> GetHighPriorityTicketsAsync()
        {
            var request = await _requestService.GetHighPriorityTicketsAsync();
            return Ok(request);
        }

        //GET /api/requests/priority-breakdown
        [HttpGet("priority-breakdown")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> GetPriorityBreakdownAsync()
        {
            var breakdown = await _requestService.GetPriorityBreakdownAsync();
            return Ok(breakdown);
        }

        //GET /api/requests/resolved-today
        [HttpGet("resolved-today")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> GetResolvedTodayCountAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);
            var count = await _requestService.GetResolvedTodayCountAsync(userId, role);

            return Ok(new
            {
                resolvedToday = count
            });
        }

        //GET /api/requests/created-today
        [HttpGet("created-today")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> GetCreatedToday()
        {
            var count = await _requestService.GetCreatedTodayAsync();
            return Ok(new { createdToday = count });
        }
    }
}
