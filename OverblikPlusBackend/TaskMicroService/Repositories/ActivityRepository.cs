using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly TaskDbContext _dbContext;

    public ActivityRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<ActivityEntity>> GetAllActivitiesAsync()
    {
        return await _dbContext.Activities
            .OrderBy(a => a.StartDateTime)
            .ToListAsync();
    }

    public async Task<List<ActivityEntity>> GetActivitiesForDateAsync(DateTime date)
    {
        return await _dbContext.Activities
            .Where(a => a.StartDateTime.Date == date.Date)
            .OrderBy(a => a.StartDateTime)
            .ToListAsync();
    }

    public async Task<List<ActivityEntity>> GetActivitiesForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbContext.Activities
            .Where(a => a.StartDateTime.Date >= startDate.Date && a.StartDateTime.Date <= endDate.Date)
            .OrderBy(a => a.StartDateTime)
            .ToListAsync();
    }

    public async Task<ActivityEntity?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ActivityEntity> AddAsync(ActivityEntity activity)
    {
        await _dbContext.Activities.AddAsync(activity);
        return activity;
    }

    public Task UpdateAsync(ActivityEntity activity)
    {
        _dbContext.Activities.Update(activity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ActivityEntity activity)
    {
        _dbContext.Activities.Remove(activity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

