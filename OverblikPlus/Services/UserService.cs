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

    public async Task<IEnumerable<ReadUserDto>> GetAllUsersAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<ReadUserDto>>("api/UserService/users");
    }

    public async Task<ReadUserDto> GetUserByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ReadUserDto>($"api/UserService/{id}");
    }

    public async Task<int> CreateUserAsync(CreateUserDto newUser)
    {
        var response = await _httpClient.PostAsJsonAsync("api/UserService", newUser);
        response.EnsureSuccessStatusCode();
        
        // Assuming the response contains the created user ID.
        var userId = await response.Content.ReadFromJsonAsync<int>();
        return userId;
    }

    public async Task UpdateUserAsync(int id, UpdateUserDto updatedUser)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserService/{id}", updatedUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/UserService/{id}");
        response.EnsureSuccessStatusCode();
    }
}