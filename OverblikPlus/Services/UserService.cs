using System.Net.Http.Json;
using OverblikPlus.Dtos.User;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ReadUserDto>> GetAllUsers()
    {
        try
        {
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<ReadUserDto>>("api/User/users");
            return users ?? new List<ReadUserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving users: {ex.Message}");
            return new List<ReadUserDto>();
        }
    }

    public async Task<ReadUserDto> GetUserById(string id)
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


    public async Task<string> CreateUser(CreateUserDto newUser)
    {
        var response = await _httpClient.PostAsJsonAsync("api/User", newUser);
        response.EnsureSuccessStatusCode();
        
        var createdUser = await response.Content.ReadFromJsonAsync<ReadUserDto>();
        
        if (createdUser != null)
        {
            return createdUser.Id;
        }

        throw new Exception("User creation failed or response was not as expected.");
    }

    public async Task UpdateUser(string id, UpdateUserDto updatedUser)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/User/{id}", updatedUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUser(string id)
    {
        var response = await _httpClient.DeleteAsync($"api/User/{id}");
        response.EnsureSuccessStatusCode();
    }
}