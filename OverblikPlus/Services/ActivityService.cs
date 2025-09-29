using OverblikPlus.Models.Dtos.Activity;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class ActivityService : IActivityService
{

    private readonly HttpClient _httpClient;

    public ActivityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<List<ActivityDto>> GetAllActivitiesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ActivityDto> GetActivityByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task CreateActivityAsync(ActivityDto activity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateActivityAsync(ActivityDto activity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteActivityAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task JoinActivityAsync(Guid activityId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task LeaveActivityAsync(Guid activityId, Guid userId)
    {
        throw new NotImplementedException();
    }
}