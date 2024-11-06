using OverblikPlus.Dtos;
using OverblikPlus.Dtos.User;

namespace OverblikPlus.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<ReadUserDto>> GetAllUsers();
    Task<ReadUserDto> GetUserById(int id);
    Task<int> CreateUser(CreateUserDto createUserDto);
    Task UpdateUser(int id, UpdateUserDto updateUserDto);
    Task DeleteUser(int id);
}