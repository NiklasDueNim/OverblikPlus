using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _dbContext;

    public TaskRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<TaskEntity>> GetAllAsync()
    {
        return await _dbContext.Tasks
            .Include(t => t.Steps)
            .ToListAsync();
    }

    // ---- Per-occurrence completion ----
    public async Task<TaskCompletion?> GetCompletionAsync(int taskId, DateTime occurrenceDate)
    {
        var day = occurrenceDate.Date;
        return await _dbContext.TaskCompletions
            .FirstOrDefaultAsync(c => c.TaskId == taskId && c.OccurrenceDate == day);
    }

    public async Task AddCompletionAsync(TaskCompletion completion)
    {
        completion.OccurrenceDate = completion.OccurrenceDate.Date;
        completion.CompletedAt = DateTime.UtcNow;
        await _dbContext.TaskCompletions.AddAsync(completion);
    }

    public async Task RemoveCompletionAsync(TaskCompletion completion)
    {
        _dbContext.TaskCompletions.Remove(completion);
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<List<TaskCompletion>> GetCompletionsForUserAsync(string userId, DateTime from, DateTime to)
    {
        var fromDay = from.Date;
        var toDay = to.Date;
        return await _dbContext.TaskCompletions
            .Where(c => c.UserId == userId && c.OccurrenceDate >= fromDay && c.OccurrenceDate <= toDay)
            .ToListAsync();
    }

    public async Task<TaskEntity?> GetByIdAsync(int id)
    {
        return await _dbContext.Tasks
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TaskEntity>> GetByUserIdAsync(string userId)
    {
        return await _dbContext.Tasks
            .Include(t => t.Steps)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetByUserIdAndDateAsync(string userId, DateTime date)
    {
        return await _dbContext.Tasks
            .Include(t => t.Steps)
            .Where(t => t.UserId == userId && t.NextOccurrence.HasValue && t.NextOccurrence.Value.Date == date.Date)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetBySeriesIdAsync(int seriesId)
    {
        return await _dbContext.Tasks
            .Where(t => t.SeriesId.HasValue && t.SeriesId.Value == seriesId)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetSameDayDuplicatesAsync(int seriesId, DateTime date, int excludeTaskId)
    {
        return await _dbContext.Tasks
            .Where(t =>
                t.Id != excludeTaskId &&
                (t.SeriesId ?? t.Id) == seriesId &&
                t.NextOccurrence.HasValue &&
                t.NextOccurrence.Value.Date == date)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetOldTasksInSeriesAsync(int seriesId, DateTime beforeDate, int excludeTaskId)
    {
        return await _dbContext.Tasks
            .Where(t =>
                t.Id != excludeTaskId &&
                (t.SeriesId ?? t.Id) == seriesId &&
                t.NextOccurrence.HasValue &&
                t.NextOccurrence.Value.Date < beforeDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsWithDateAsync(int seriesId, DateTime date)
    {
        return await _dbContext.Tasks
            .AnyAsync(t =>
                (t.SeriesId ?? t.Id) == seriesId &&
                t.NextOccurrence.HasValue &&
                t.NextOccurrence.Value.Date == date);
    }

    public async Task<TaskEntity> AddAsync(TaskEntity task)
    {
        await _dbContext.Tasks.AddAsync(task);
        return task;
    }

    public Task UpdateAsync(TaskEntity task)
    {
        _dbContext.Tasks.Update(task);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskEntity task)
    {
        _dbContext.Tasks.Remove(task);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<TaskEntity> tasks)
    {
        _dbContext.Tasks.RemoveRange(tasks);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

