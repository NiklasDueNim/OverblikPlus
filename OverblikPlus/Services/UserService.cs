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
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<ReadUserDto>>("api/UserService/users");
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
            var user = await _httpClient.GetFromJsonAsync<ReadUserDto>($"api/UserService/{id}");
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
        var response = await _httpClient.PostAsJsonAsync("api/UserService", newUser);
        response.EnsureSuccessStatusCode();
        
        // Assuming the API returns the created user's ID as a JSON response like: { "id": "some-id" }
        var createdUser = await response.Content.ReadFromJsonAsync<ReadUserDto>();
        
        if (createdUser != null)
        {
            return createdUser.Id;
        }

        throw new Exception("User creation failed or response was not as expected.");
    }

    public async Task UpdateUser(string id, UpdateUserDto updatedUser)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserService/{id}", updatedUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUser(string id)
    {
        var response = await _httpClient.DeleteAsync($"api/UserService/{id}");
        response.EnsureSuccessStatusCode();
    }
}