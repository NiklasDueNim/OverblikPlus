namespace OverblikPlus.Services;

public interface IUserService
{
    Task<IEnumerable<ReadUserDto>> GetAllUsers();
    Task<ReadUserDto> GetUserById(int id);
    Task<int> CreateUser(CreateUserDto createUserDto);
    Task UpdateUser(int id, UpdateUserDto updateUserDto);
    Task DeleteUser(int id);
}