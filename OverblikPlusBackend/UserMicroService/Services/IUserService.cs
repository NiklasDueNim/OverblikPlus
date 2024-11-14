using UserMicroService.dto;

namespace UserMicroService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ReadUserDto>> GetAllUsersAsync();
        Task<ReadUserDto> GetUserById(string id, string userRole);
        Task<string> CreateUserAsync(CreateUserDto createUserDto);
        Task DeleteUserAsync(string id);
        Task UpdateUserAsync(string id, UpdateUserDto updateUserDto);
    }
}