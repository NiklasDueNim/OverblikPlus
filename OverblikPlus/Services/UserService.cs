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
        return await _httpClient.GetFromJsonAsync<IEnumerable<ReadUserDto>>("api/UserService/users");
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
        var userId = await response.Content.ReadFromJsonAsync<int>();
        return userId;
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