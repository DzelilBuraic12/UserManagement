using UserManagement.DTOs;

namespace UserManagement.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RefreshAsync(string refreshToken);
        Task RegisterAsync(RegisterDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<bool> ResetPasswordStartAsync(string email);
        Task<bool> ResetPasswordCompleteAsync(ResetPasswordDto dto);
    }
}
