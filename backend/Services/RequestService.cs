using UserManagement.Contracts;
using UserManagement.DTOs;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Common;
using FluentValidation;
using FluentValidation.Results;
using UserManagement.Domain.Entities;
using UserManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using UserManagement.Domain.Constants;
using UserManagement.Validation;
using System.Security.Claims;


namespace UserManagement.Services
{

    public class RequestService : IRequestService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IValidator<RequestCreateDto> _validator;
        IValidator<RequestUpdateDto> _updateValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestService(
            IValidator<RequestCreateDto> validator,
            IValidator<RequestUpdateDto> updateValidator,
            IMapper mapper, AppDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _validator = validator;
            _updateValidator = updateValidator;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<RequestDetailsDto> CreateAsync(RequestCreateDto dto, int userId)
        {
            ValidationResult validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var request = _mapper.Map<Request>(dto);

            request.CreatedById = userId;
            request.StatusId = StatusIDs.Open;
            request.TechnicianId = null;

            await _db.Requests.AddAsync(request);
            await _db.SaveChangesAsync();

            await _db.Entry(request).Reference(r => r.Status).LoadAsync();
            await _db.Entry(request).Reference(r => r.CreatedBy).LoadAsync();
            await _db.Entry(request).Reference(r => r.Technician).LoadAsync();

            var result = _mapper.Map<RequestDetailsDto>(request);

            return result;


        }

        public async Task<RequestDetailsDto?> GetByIdAsync(int id)
        {

            var request = await _db.Requests
                .Include(r => r.Status)
                .Include(r => r.CreatedBy)
                .Include(r => r.Technician)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return null;

            var dto = _mapper.Map<RequestDetailsDto>(request);
            return dto;

        }

        public async Task<PagedResult<RequestListDto>> ListAsync(RequestQuery query)
        {
            IQueryable<UserManagement.Domain.Entities.Request> q = _db.Requests.AsNoTracking();
            q = q.Include(r => r.Status)
                .Include(r => r.Technician);


            if (query.IncludeClosed != true)
            {
                q = q.Where(r => r.StatusId != 4);
            }
            if (query.StatusId.HasValue)
            {
                q = q.Where(r => r.StatusId == query.StatusId.Value);
            }

            if (query.TechnicianId.HasValue)
            {
                q = q.Where(r => r.TechnicianId == query.TechnicianId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                q = q.Where(r => r.Title.ToLower().Contains(term)
                  || r.Description.ToLower().Contains(term));
            }
            if (query.CreatedById.HasValue)
            {
                q = q.Where(r => r.CreatedById == query.CreatedById.Value);
            }

            var userIdStr = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdStr, out var id) ? id : 0;

            if (!string.IsNullOrEmpty(query.MyAssignedOnly) && query.MyAssignedOnly == "true")
            {
                q = q.Where(r => r.TechnicianId == currentUserId);
            }
            var sortBy = (query.SortBy ?? "CreatedAt").ToLowerInvariant();
            bool asc = string.Equals(query.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "priority" => asc ? q.OrderBy(r => r.Priority) : q.OrderByDescending(r => r.Priority),
                "title" => asc ? q.OrderBy(r => r.Title) : q.OrderByDescending(r => r.Title),
                _ => asc ? q.OrderBy(r => r.CreatedAt) : q.OrderByDescending(r => r.CreatedAt)
            };


            int page = Math.Max(1, query.Page);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);
            int skip = (page - 1) * pageSize;

            var total = await q.CountAsync();
            var items = await q.Skip(skip).Take(pageSize).ToListAsync();

            var data = _mapper.Map<List<RequestListDto>>(items);

            return new PagedResult<RequestListDto>
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize
            };

        }

        public async Task<bool> AssignTechnicianAsync(int id, int technicianId, int performedByUserId)
        {
            var request = await _db.Requests.FirstOrDefaultAsync(r => r.Id == id);

            var performedBy = await _db.Users.FirstOrDefaultAsync(u => u.Id == performedByUserId);

            if (request == null)
            {
                return false;
            }


            if (request.StatusId == StatusIDs.Closed)
            {
                return false;
            }

            if (request.TechnicianId.HasValue)
            {
                return false;
            }

            var tech = await _db.Users.FirstOrDefaultAsync(u => u.Id == technicianId);

            if (tech == null)
            {
                return false;
            }
            else if (!tech.IsActive)
            {
                return false;
            }
            else if (!string.Equals(tech.Role, Roles.Technician, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            request.TechnicianId = technicianId;



            if (performedBy == null)
                return false;
            if (!performedBy.IsActive)
                return false;
            if (!string.Equals(performedBy.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
                return false;

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<RequestSummaryDto> GetSummaryAsync(int userId, string role)
        {
            IQueryable<UserManagement.Domain.Entities.Request> q = _db.Requests.AsNoTracking();

            if (string.Equals(role, Roles.User, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.CreatedById == userId);
            }
            else if (string.Equals(role, Roles.Technician, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.TechnicianId == userId);
            }

            return new RequestSummaryDto
            {
                Open = await q.CountAsync(r => r.StatusId == StatusIDs.Open),
                InProgress = await q.CountAsync(r => r.StatusId == StatusIDs.InProgress),
                Resolved = await q.CountAsync(r => r.StatusId == StatusIDs.Resolved),
                Closed = await q.CountAsync(r => r.StatusId == StatusIDs.Closed)

            };
        }

        public async Task<PriorityBreakdownDto> GetPriorityBreakdownAsync()
        {
            var high = await _db.Requests.CountAsync(r => r.Priority == Request.RequestPriority.High);
            var normal = await _db.Requests.CountAsync(r => r.Priority == Request.RequestPriority.Normal);
            var low = await _db.Requests.CountAsync(r => r.Priority == Request.RequestPriority.Low);

            return new PriorityBreakdownDto
            {
                High = high,
                Normal = normal,
                Low = low
            };

        }

        public async Task<int> GetResolvedTodayCountAsync(int userId, string role)
        {
            var today = DateTime.UtcNow.Date;

            IQueryable<Request> q = _db.Requests.AsNoTracking();

            if(string.Equals(role, Roles.User, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.CreatedById == userId);
            }

            return await q
                .Where(r => r.StatusId == StatusIDs.Resolved
                        && r.UpdatedAt.HasValue
                        && r.UpdatedAt.Value.Date == today)
                .CountAsync();
        }

        public async Task<List<HighPriorityRequestDto>> GetHighPriorityTicketsAsync()
        {
            IQueryable<UserManagement.Domain.Entities.Request> q = _db.Requests.Include(r => r.Technician);
            q = q.Where(r => r.Priority == Request.RequestPriority.High && r.StatusId != StatusIDs.Closed);

            var tickets = await q.OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            var request = tickets.Select(r => new HighPriorityRequestDto
            {
                Id = r.Id,
                Title = r.Title,
                AssignedTo = r.Technician != null ? $"{r.Technician.FirstName} {r.Technician.LastName}"
                : "Unassigned",
                Age = FormatTimeAgo(r.CreatedAt),
            }).ToList();

            return request;
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int userId, string role)
        {

            IQueryable<UserManagement.Domain.Entities.Request> q = _db.Requests.AsNoTracking();
            q = q.Include(r => r.Status)
                .Include(r => r.Technician)
                .Include(r => r.CreatedBy);

            if(string.Equals(role, Roles.User, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.CreatedById == userId);
            }

            var recent = await q.OrderByDescending(r => r.CreatedAt).Take(5).ToListAsync();

            var result = recent.Select(r => new RecentActivityDto 
            {
                Id = r.Id,
                TicketId = r.Id,
                Message = BuildActivityMessage(r),
                Time = FormatTimeAgo(r.CreatedAt),
                Type = MapStatusToType(r.StatusId)
            }).ToList();

            return result;
        }

        private string MapStatusToType(int statusId) 
        {
            switch (statusId)
            {
                case StatusIDs.Open:
                    return "new";
                case StatusIDs.InProgress:
                    return "progress";
                case StatusIDs.Resolved:
                    return "resolved";
                case StatusIDs.Closed:
                    return "closed";
                default:
                    return "new";
            }
        }

        private string BuildActivityMessage(Request r)
        {
            var creatorName = r.CreatedBy.FirstName + " " + r.CreatedBy.LastName;
            switch (r.StatusId)
            {
                case StatusIDs.Open:
                    return $"New ticket #{r.Id} created by {creatorName}";
                case StatusIDs.InProgress:
                    var techName = r.Technician != null ? r.Technician.FirstName : "technician";
                    return $"Ticket #{r.Id} assigned to {techName}";
                case StatusIDs.Resolved:
                    return $"Ticket #{r.Id} resolved";
                case StatusIDs.Closed:
                    return $"Ticket #{r.Id} closed";
                default:
                    return $"Ticket #{r.Id} updated";
            }
        }
        private string FormatTimeAgo(DateTime dateTime)
        {
            var dateDiff = DateTime.UtcNow - dateTime;
            
            if( dateDiff.TotalMinutes < 1)
            {
                return "Just now";
            }

            if( dateDiff.TotalMinutes < 60)
            {
                return $"{(int)dateDiff.TotalMinutes} minutes ago";
            }
            if(dateDiff.TotalHours < 24)
            {
                return $"{(int)dateDiff.TotalHours} hours ago";
            }
            if(dateDiff.TotalDays < 7)
            {
                return $"{(int)dateDiff.TotalDays} days ago";
            }
            else
            {
                return dateTime.ToString("MMM dd");
            }
        }

        public async Task<int> GetCreatedTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            var count = await _db.Requests 
                .Where(r => r.CreatedAt.Date == today)
                .CountAsync();

            return count;
        }

        public async Task<DashboardSummaryWithTrendsDto> GetDashboardSummaryWithTrendsAsync(int userId, string role)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            IQueryable<Request> q = _db.Requests.AsNoTracking();

            if (string.Equals(role, Roles.User, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.CreatedById == userId);
            }
            else if (string.Equals(role, Roles.Technician, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.TechnicianId == userId);
            }

            var open = await q.CountAsync(r => r.StatusId == StatusIDs.Open);
            var inProgress = await q.CountAsync(r => r.StatusId == StatusIDs.InProgress);
            var resolved = await q.CountAsync(r => r.StatusId == StatusIDs.Resolved);
            var closed = await q.CountAsync(r => r.StatusId == StatusIDs.Closed);

            var openTrend = await CalculateTrendForStatus(StatusIDs.Open, today, yesterday, userId, role);
            var inProgressTrend = await CalculateTrendForStatus(StatusIDs.InProgress, today, yesterday, userId, role);
            var resolvedTrend = await CalculateTrendForStatus(StatusIDs.Resolved, today, yesterday, userId, role);
            var closedTrend = await CalculateTrendForStatus(StatusIDs.Closed, today, yesterday, userId, role);

            return new DashboardSummaryWithTrendsDto
            {
                Open = open,
                InProgress = inProgress,
                Resolved = resolved,
                Closed = closed,
                Trends = new TrendsDto
                {
                    Open = openTrend,
                    InProgress = inProgressTrend,
                    Resolved = resolvedTrend,
                    Closed = closedTrend
                }
            };
        }

        private async Task<int> CalculateTrendForStatus(int statusId, DateTime today, DateTime yesterday, int userId, string role)
        {
            IQueryable<Request> q = _db.Requests.AsNoTracking();

            if (string.Equals(role, Roles.User, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.CreatedById == userId);
            }
            else if (string.Equals(role, Roles.Technician, StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.TechnicianId == userId);
            }

            var todayCount = await q.CountAsync(r => r.StatusId == statusId && r.CreatedAt.Date == today);
            var yesterdayCount = await q.CountAsync(r => r.StatusId == statusId && r.CreatedAt.Date == yesterday);

            return todayCount - yesterdayCount;
        }


        public async Task<bool> ChangeStatusAsync(int id, int newStatusId, int performedByUserId)
        {
            var performedBy = await _db.Users.FirstOrDefaultAsync(u => u.Id == performedByUserId);

            if (performedBy == null)
                return false;
            if (!performedBy.IsActive)
                return false;


            var requests = await _db.Requests.FirstOrDefaultAsync(r => r.Id == id);

            if (requests == null) return false;

            if (newStatusId != StatusIDs.Open && newStatusId != StatusIDs.InProgress
                && newStatusId != StatusIDs.Resolved && newStatusId != StatusIDs.Closed)
                return false;

            if (requests.StatusId == StatusIDs.Closed)
                return false;

            var current = requests.StatusId;
            var target = newStatusId;
            bool allowed = false;
            switch (current)
            {
                case StatusIDs.Open:
                    if (target == StatusIDs.InProgress
                        && requests.TechnicianId.HasValue
                        && (string.Equals(performedBy.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(performedBy.Role, Roles.Technician, StringComparison.OrdinalIgnoreCase)))
                    {
                        allowed = true;
                        requests.StatusId = target;
                    }
                    break;
                case StatusIDs.InProgress:
                    if (target == StatusIDs.Resolved
                        && (string.Equals(performedBy.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(performedBy.Role, Roles.Technician, StringComparison.OrdinalIgnoreCase)))
                    {
                        allowed = true;
                        requests.StatusId = target;
                    }
                    break;
                case StatusIDs.Resolved:
                    if (target == StatusIDs.Closed
                        && (string.Equals(performedBy.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase)))
                    {
                        allowed = true;
                        requests.StatusId = target;

                    }
                    break;
                case StatusIDs.Closed:
                    allowed = false;
                    break;

                default:
                    allowed = false;
                    break;

            }

            if (!allowed)
                return false;

            requests.UpdatedAt = DateTime.UtcNow;


            var saved = await _db.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<int> UpdateAsync(int id, RequestUpdateDto dto, int performedByUserId)
        {
            var performedBy = await _db.Users.FirstOrDefaultAsync(x => x.Id == performedByUserId);
            if (performedBy == null || !performedBy.IsActive)
                return 0; 

            var request = await _db.Requests.FirstOrDefaultAsync(x => x.Id == id);
            if (request == null)
                return -1; 

            if (request.StatusId == StatusIDs.Closed)
                return 0;

            var isAdmin = string.Equals(performedBy.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase);
            var isCreator = request.CreatedById == performedByUserId;
            if (!(isAdmin || isCreator))
                return 0;

            var result = await _updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            var changed = false;

            if (dto.StatusId.HasValue && dto.StatusId != request.StatusId)
            {
                request.StatusId = dto.StatusId.Value;
                changed = true;
            }

            if (dto.Title != null && dto.Title.Trim() != request.Title)
            {
                request.Title = dto.Title.Trim();
                changed = true;
            }

            if (dto.Description != null && dto.Description.Trim() != request.Description)
            {
                request.Description = dto.Description.Trim();
                changed = true;
            }

            if (dto.DueDate.HasValue && dto.DueDate != request.DueDate)
            {
                request.DueDate = dto.DueDate;
                changed = true;
            }

            if (changed)
            {
                request.UpdatedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(dto.Priority))
            {
                if (!Enum.TryParse<Request.RequestPriority>(dto.Priority, true, out var prio))
                    throw new ValidationException("Priority must be Low, Normal or High");

                if (prio != request.Priority)
                {
                    request.Priority = prio;
                    changed = true;
                }
            }

            if (!changed)
                return 1; 

            var saved = await _db.SaveChangesAsync();
            return saved > 0 ? 1 : 0;
        }
    }
}

