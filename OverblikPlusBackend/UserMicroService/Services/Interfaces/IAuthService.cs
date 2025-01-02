using UserMicroService.Common;
using UserMicroService.dto;

namespace UserMicroService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<(string, string)>> LoginAsync(LoginDto loginDto);
        Task<Result> RegisterAsync(RegisterDto registerDto);
        Task<Result<string>> RefreshTokenAsync(string token);
        Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<Result> LogoutAsync();
    }
}