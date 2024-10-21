namespace OverblikPlus.Services;

public interface IUserService
{
    Task<IEnumerable<ReadUserDto>> GetAllUsersAsync();
    Task<ReadUserDto> GetUserByIdAsync(int id);
    Task<int> CreateUserAsync(CreateUserDto createUserDto);
    Task UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task DeleteUserAsync(int id);
}