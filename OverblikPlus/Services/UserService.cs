using System.Net.Http.Json;
using System.Linq;
using Microsoft.Extensions.Logging;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.User;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;

    public UserService(HttpClient httpClient, ILogger<UserService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<ReadUserDto>>> GetAllUsers()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/User/users");
            
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<IEnumerable<ReadUserDto>>();
                if (users != null)
                {
                    return Result<IEnumerable<ReadUserDto>>.SuccessResult(users);
                }
            }
            
            return Result<IEnumerable<ReadUserDto>>.ErrorResult("Kunne ikke hente brugere");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return Result<IEnumerable<ReadUserDto>>.ErrorResult($"Fejl ved hentning af brugere: {ex.Message}");
        }
    }

    public async Task<Result<ReadUserDto>> GetUserById(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/User/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<ReadUserDto>();
                if (user != null)
                {
                    return Result<ReadUserDto>.SuccessResult(user);
                }
            }
            
            return Result<ReadUserDto>.ErrorResult($"Bruger med ID {id} blev ikke fundet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return Result<ReadUserDto>.ErrorResult($"Fejl ved hentning af bruger: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ReadUserDto>>> GetUsersByBostedId(int bostedId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Result<List<ApplicationUserDto>>>($"api/User/bosted/{bostedId}");
            if (response != null && response.Success && response.Data != null)
            {
                // Map ApplicationUserDto to ReadUserDto
                var users = response.Data.Select(u => new ReadUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.UserName ?? u.Email,
                    Role = u.Role
                });
                return Result<IEnumerable<ReadUserDto>>.SuccessResult(users);
            }
            
            return Result<IEnumerable<ReadUserDto>>.ErrorResult(
                response?.Error ?? $"Kunne ikke hente brugere for bosted {bostedId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for bosted {BostedId}", bostedId);
            return Result<IEnumerable<ReadUserDto>>.ErrorResult($"Fejl ved hentning af brugere: {ex.Message}");
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
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User created successfully");
                return Result.SuccessResult();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to create user. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            return Result.ErrorResult($"Kunne ikke oprette bruger: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Result.ErrorResult($"Fejl ved oprettelse af bruger: {ex.Message}");
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

    public async Task<Result> UpdateUser(string id, UpdateUserDto updatedUser)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/User/{id}", updatedUser);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User {UserId} updated successfully", id);
                return Result.SuccessResult();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to update user {UserId}. Status: {StatusCode}, Error: {Error}", 
                id, response.StatusCode, errorContent);
            return Result.ErrorResult($"Kunne ikke opdatere bruger: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return Result.ErrorResult($"Fejl ved opdatering af bruger: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUser(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/User/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User {UserId} deleted successfully", id);
                return Result.SuccessResult();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete user {UserId}. Status: {StatusCode}, Error: {Error}", 
                id, response.StatusCode, errorContent);
            return Result.ErrorResult($"Kunne ikke slette bruger: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return Result.ErrorResult($"Fejl ved sletning af bruger: {ex.Message}");
        }
    }
}