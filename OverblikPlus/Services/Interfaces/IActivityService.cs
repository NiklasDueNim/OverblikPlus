using OverblikPlus.Models.Dtos.Activity;

namespace OverblikPlus.Services.Interfaces;

public interface IActivityService
{
    Task<List<ActivityDto>> GetAllActivitiesAsync();
    Task<ActivityDto?> GetActivityByIdAsync(Guid id);
    Task<bool> CreateActivityAsync(CreateActivityDto activity);
    Task<bool> UpdateActivityAsync(Guid id, CreateActivityDto activity);
    Task<bool> DeleteActivityAsync(Guid id);
    Task<ApiResult> JoinActivityAsync(Guid activityId, string userId);
    Task<ApiResult> LeaveActivityAsync(Guid activityId, string userId);
}

public class ApiResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}