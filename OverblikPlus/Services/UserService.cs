using System.Net.Http.Json;
using System.Linq;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.User;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IEnumerable<ReadUserDto>> GetAllUsers()
    {
        try
        {
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<ReadUserDto>>("api/User/users");
            if (users == null)
            {
                throw new Exception("No users received.");
            }
            return users;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving users: {ex.Message}");
            return new List<ReadUserDto>();
        }
    }

    public async Task<ReadUserDto?> GetUserById(string id)
    {
        try
        {
            var user = await _httpClient.GetFromJsonAsync<ReadUserDto>($"api/User/{id}");
            if (user == null)
            {
                Console.WriteLine($"User with ID {id} not found.");
            }
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user with ID {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<ReadUserDto>> GetUsersByBostedId(int bostedId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Result<List<ApplicationUserDto>>>($"api/User/bosted/{bostedId}");
            if (response != null && response.Success && response.Data != null)
            {
                // Map ApplicationUserDto to ReadUserDto
                return response.Data.Select(u => new ReadUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.UserName ?? u.Email,
                    Role = u.Role
                });
            }
            return new List<ReadUserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving users for bosted {bostedId}: {ex.Message}");
            return new List<ReadUserDto>();
        }
    }

    public async Task<Result> CreateUser(CreateUserDto newUser)
    {
        try
        {
            // Map danske rolle-navne til engelske navne før de sendes til backend
            var mappedUser = new CreateUserDto
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Password = newUser.Password,
                Role = MapRoleToEnglish(newUser.Role)
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", mappedUser);
            response.EnsureSuccessStatusCode();
            
            return new Result { Success = true };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            throw;
        }
    }

    private string MapRoleToEnglish(string? role)
    {
        return role switch
        {
            "Beboer" => "User",
            "Medarbejder" => "Staff",
            "Admin" => "Admin",
            "Relative" => "Relative",
            _ => role ?? "User" // Default til User hvis rolle er null eller ukendt
        };
    }

    public async Task UpdateUser(string id, UpdateUserDto updatedUser)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/User/{id}", updatedUser);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user with ID {id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteUser(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/User/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user with ID {id}: {ex.Message}");
            throw;
        }
    }
}