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



namespace UserManagement.Services
{

    public class RequestService : IRequestService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IValidator<RequestCreateDto> _validator;
        IValidator<RequestUpdateDto> _updateValidator;

        public RequestService(
            IValidator<RequestCreateDto> validator,
            IValidator<RequestUpdateDto> updateValidator,
            IMapper mapper, AppDbContext db)
        {
            _db = db;
            _validator = validator;
            _updateValidator = updateValidator;
            _mapper = mapper;
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
            if (request.StatusId == StatusIDs.Open)
            {
                request.StatusId = StatusIDs.InProgress;
            }


            if (performedBy == null)
                return false;
            if (!performedBy.IsActive)
                return false;
            if (!string.Equals(performedBy.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
                return false;

            await _db.SaveChangesAsync();

            return true;
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
                return 0;

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

            if (dto.Title != null)
            {
                request.Title = dto.Title.Trim();
                changed = true;
            }

            if(dto.Description != null)
            {
                request.Description = dto.Description.Trim();
                changed = true;
            }
            
            if(dto.DueDate.HasValue)
            {
                request.DueDate = dto.DueDate;
                changed = true;
            }

            if(!string.IsNullOrEmpty(dto.Priority))
            {
                if (!Enum.TryParse<Request.RequestPriority>(dto.Priority, true, out var prio))
                    throw new ValidationException("Priority must be Low, Normal or High");
                request.Priority = prio;
                changed = true;
            }
            

            if (changed == false)
                return 0;

            var saved = await _db.SaveChangesAsync();
            return saved;
        }
    }
}
