using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface IActivityRepository
{
    Task<List<ActivityEntity>> GetAllActivitiesAsync();
    Task<List<ActivityEntity>> GetActivitiesForDateAsync(DateTime date);
    Task<List<ActivityEntity>> GetActivitiesForDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ActivityEntity?> GetByIdAsync(Guid id);
    Task<ActivityEntity> AddAsync(ActivityEntity activity);
    Task UpdateAsync(ActivityEntity activity);
    Task DeleteAsync(ActivityEntity activity);
    Task SaveChangesAsync();
}

