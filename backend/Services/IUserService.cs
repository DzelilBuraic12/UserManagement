using UserManagement.Common;
using UserManagement.Contracts;
using UserManagement.DTOs;

namespace UserManagement.Services
{
    public interface IUserService
    {
        Task<UserReadDto?> GetByIdAsync(int id);
        Task<PagedResult<UserListDto>> ListAsync(UserQuery query);
        Task<int> CreateAsync(UserCreateDto dto);
        Task<bool> UpdateAsync(int id, UserUpdateDto dto, int performedByUserId);
        Task<bool> DeactivateAsync(int id, int performedByUserId);
        Task<bool> AssignRoleAsync(int id, string role, int performedByUserId);
        Task<bool> DeleteAsync(int id, int performedByUserId);
        Task<UserReadDto?> GetCurrentAsync(int userId); 
    }
}
