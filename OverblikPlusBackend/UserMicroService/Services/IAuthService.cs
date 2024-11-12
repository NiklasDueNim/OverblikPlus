using UserMicroService.dto;

namespace UserMicroService.Services;

public interface IAuthService
{
    Task<string?> AuthenticateAsync(LoginDto loginDto);
}