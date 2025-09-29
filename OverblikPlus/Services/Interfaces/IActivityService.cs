using OverblikPlus.Models.Dtos.Activity;

namespace OverblikPlus.Services.Interfaces;

public interface IActivityService
{
    Task<List<ActivityDto>> GetAllActivitiesAsync();
    Task<ActivityDto> GetActivityByIdAsync(Guid id);
    Task CreateActivityAsync(ActivityDto activity);
    Task UpdateActivityAsync(ActivityDto activity);
    Task DeleteActivityAsync(Guid id);

    Task JoinActivityAsync(Guid activityId, Guid userId);
    Task LeaveActivityAsync(Guid activityId, Guid userId);
    
}