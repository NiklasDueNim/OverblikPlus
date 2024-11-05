using System.Net.Http.Json;
using OverblikPlus;
using OverblikPlus.Services;

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


    public async Task<ReadUserDto> GetUserById(int id)
    {
        return await _httpClient.GetFromJsonAsync<ReadUserDto>($"api/UserService/{id}");
    }

    public async Task<int> CreateUser(CreateUserDto newUser)
    {
        var response = await _httpClient.PostAsJsonAsync("api/UserService", newUser);
        response.EnsureSuccessStatusCode();
        
        // Assuming the response contains the created user ID.
        var createdUser= await response.Content.ReadFromJsonAsync<CreateUserDto>();
        return createdUser.Id;
    }

    public async Task UpdateUser(int id, UpdateUserDto updatedUser)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserService/{id}", updatedUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUser(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/UserService/{id}");
        response.EnsureSuccessStatusCode();
    }
}