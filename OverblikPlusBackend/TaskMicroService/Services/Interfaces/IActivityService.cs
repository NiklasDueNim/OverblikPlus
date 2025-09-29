using TaskMicroService.Common;
using TaskMicroService.dtos.Activity;

namespace TaskMicroService.Services.Interfaces;

public interface IActivityService
{
    Task<Result<IEnumerable<ReadActivityDto>>> GetAllActivitiesAsync();
    Task<Result<IEnumerable<ReadActivityDto>>> GetActivitiesForDateAsync(DateTime date);
    Task<Result<IEnumerable<ReadActivityDto>>> GetActivitiesForDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Result<ReadActivityDto>> GetActivityByIdAsync(Guid id);
    Task<Result<Guid>> CreateActivityAsync(CreateActivityDto createActivityDto);
    Task<Result> UpdateActivityAsync(Guid id, CreateActivityDto updateActivityDto);
    Task<Result> DeleteActivityAsync(Guid id);
    Task<Result> JoinActivityAsync(Guid activityId, Guid userId);
    Task<Result> LeaveActivityAsync(Guid activityId, Guid userId);
    Task<Result<bool>> CanUserJoinActivityAsync(Guid activityId, Guid userId);
}
