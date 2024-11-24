using OverblikPlus.Dtos.User;

namespace OverblikPlus.Services.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> RegisterAsync(CreateUserDto createUserDto);
    Task<bool> RefreshTokenAsync();
}