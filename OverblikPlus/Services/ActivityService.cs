using OverblikPlus.Models.Dtos.Activity;
using OverblikPlus.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace OverblikPlus.Services;

public class ActivityService : IActivityService
{
    private readonly HttpClient _httpClient;

    public ActivityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ActivityDto>> GetAllActivitiesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Activity");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<List<ActivityDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Data ?? new List<ActivityDto>();
            }
            return new List<ActivityDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting activities: {ex.Message}");
            return new List<ActivityDto>();
        }
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Activity/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<ActivityDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Data;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting activity {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CreateActivityAsync(CreateActivityDto activity)
    {
        try
        {
            var json = JsonSerializer.Serialize(activity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/Activity", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating activity: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateActivityAsync(Guid id, CreateActivityDto activity)
    {
        try
        {
            var json = JsonSerializer.Serialize(activity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"api/Activity/{id}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating activity {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteActivityAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Activity/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting activity {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<ApiResult> JoinActivityAsync(Guid activityId, string userId)
    {
        try
        {
            var request = new { UserId = userId };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"api/Activity/{activityId}/join", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<ApiResult>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result ?? new ApiResult { Success = false, ErrorMessage = "Unknown error" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error joining activity {activityId}: {ex.Message}");
            return new ApiResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResult> LeaveActivityAsync(Guid activityId, string userId)
    {
        try
        {
            var request = new { UserId = userId };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"api/Activity/{activityId}/leave", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<ApiResult>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result ?? new ApiResult { Success = false, ErrorMessage = "Unknown error" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error leaving activity {activityId}: {ex.Message}");
            return new ApiResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
}