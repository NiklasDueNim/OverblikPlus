using OverblikPlus.Dtos;
using OverblikPlus.Dtos.User;

namespace OverblikPlus.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<ReadUserDto>> GetAllUsers();
    Task<ReadUserDto> GetUserById(string id);
    Task<string> CreateUser(CreateUserDto createUserDto);
    Task UpdateUser(string id, UpdateUserDto updateUserDto);
    Task DeleteUser(string id);
}