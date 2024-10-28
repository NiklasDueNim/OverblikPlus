using UserMicroService.dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserMicroService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ReadUserDto>> GetAllUsersAsync(); // Asynkron metode
        Task<ReadUserDto> GetUserById(int id, string userRole); // Asynkron metode og brugerrolle til følsomme data
        Task<int> CreateUserAsync(CreateUserDto createUserDto); // Asynkron metode
        Task DeleteUserAsync(int id); // Asynkron metode
        Task UpdateUserAsync(int id, UpdateUserDto updateUserDto); // Asynkron metode
    }
}