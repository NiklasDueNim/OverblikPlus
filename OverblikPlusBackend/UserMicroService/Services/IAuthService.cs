using UserMicroService.dto;

namespace UserMicroService.Services;

public interface IAuthService
{
    Task<(string, string)> LoginAsync(LoginDto loginDto);
    Task<RegistrationResult> RegisterAsync(RegisterDto registerDto);
    Task<string> RefreshTokenAsync(string token);
    Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task LogoutAsync();
}