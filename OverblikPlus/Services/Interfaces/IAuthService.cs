using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Auth;
using OverblikPlus.Models.Dtos.User;

namespace OverblikPlus.Services.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> RegisterAsync(CreateUserDto createUserDto);
}