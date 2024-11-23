namespace AuthMicroService;

public interface IAuthService
{
    Task<(string Token, string RefreshToken)> LoginAsync(LoginDto loginDto);
    Task<RegistrationResult> RegisterAsync(RegisterDto registerDto);
    Task<string> RefreshTokenAsync(string token);
    Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
}
