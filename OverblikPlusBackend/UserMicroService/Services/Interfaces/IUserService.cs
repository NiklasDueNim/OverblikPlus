using System.Collections.Generic;
using System.Threading.Tasks;
using UserMicroService.Common;
using UserMicroService.dto;
using UserMicroService.Entities;

namespace UserMicroService.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<IEnumerable<ReadUserDto>>> GetAllUsersAsync();
        Task<Result<ReadUserDto>> GetUserById(string id, string userRole);
        Task<Result<string>> CreateUserAsync(CreateUserDto createUserDto);
        Task<Result> DeleteUserAsync(string id);
        Task<Result> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task<Result<List<ApplicationUser>>> GetAllUsersForBosted(int bostedId);   
    }
}