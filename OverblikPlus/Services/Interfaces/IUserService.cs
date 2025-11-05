using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.User;

namespace OverblikPlus.Services.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<ReadUserDto>>> GetAllUsers();
    Task<Result<ReadUserDto>> GetUserById(string id);
    Task<Result<IEnumerable<ReadUserDto>>> GetUsersByBostedId(int bostedId);
    Task<Result> CreateUser(CreateUserDto newUser);
    Task<Result> UpdateUser(string id, UpdateUserDto updateUserDto);
    Task<Result> DeleteUser(string id);
}