using System.Net.Http.Json;
using OverblikPlus.Common;
using OverblikPlus.Dtos.User;
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

    public async Task<Result> CreateUser(CreateUserDto newUser)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", newUser);
            response.EnsureSuccessStatusCode();

            var createdUser = await response.Content.ReadFromJsonAsync<ReadUserDto>();
            if (createdUser != null)
            {
                return new Result { Success = true };
            }

            throw new Exception("User creation failed or response was not as expected.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            throw;
        }
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