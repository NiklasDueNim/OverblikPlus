using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskEntity>> GetAllAsync();
    Task<TaskEntity?> GetByIdAsync(int id);
    Task<List<TaskEntity>> GetByUserIdAsync(string userId);
    Task<List<TaskEntity>> GetByUserIdAndDateAsync(string userId, DateTime date);
    Task<List<TaskEntity>> GetBySeriesIdAsync(int seriesId);
    Task<List<TaskEntity>> GetSameDayDuplicatesAsync(int seriesId, DateTime date, int excludeTaskId);
    Task<List<TaskEntity>> GetOldTasksInSeriesAsync(int seriesId, DateTime beforeDate, int excludeTaskId);
    Task<bool> ExistsWithDateAsync(int seriesId, DateTime date);
    Task<TaskEntity> AddAsync(TaskEntity task);
    Task UpdateAsync(TaskEntity task);
    Task DeleteAsync(TaskEntity task);
    Task DeleteRangeAsync(IEnumerable<TaskEntity> tasks);
    Task SaveChangesAsync();
}

