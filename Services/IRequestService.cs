using UserManagement.Contracts;
using UserManagement.Common;
using UserManagement.DTOs;

namespace UserManagement.Services
{
    public interface IRequestService
    {
        Task<RequestDetailsDto> CreateAsync(RequestCreateDto dto, int userId);
        Task<PagedResult<RequestListDto>> ListAsync(RequestQuery query);
        Task<RequestDetailsDto?> GetByIdAsync(int id);
        Task<bool> AssignTechnicianAsync(int id, int technicianId, int performedByUserId);
        Task<bool> ChangeStatusAsync(int id, int newStatusId, int performedByUserId);
        Task<int> UpdateAsync(int id, RequestUpdateDto dto, int performedByUserId);
        

    }
}
