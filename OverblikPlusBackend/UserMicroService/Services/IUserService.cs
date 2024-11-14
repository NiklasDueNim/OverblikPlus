using UserMicroService.dto;

namespace UserMicroService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ReadUserDto>> GetAllUsersAsync();
        Task<ReadUserDto> GetUserById(int id, string userRole);
        Task<string> CreateUserAsync(CreateUserDto createUserDto);
        Task DeleteUserAsync(int id);
        Task UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    }
}