using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class MoodRepository : IMoodRepository
{
    private readonly TaskDbContext _dbContext;

    public MoodRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<MoodEntity?> GetMoodByUserIdAndDateAsync(string userId, DateTime date)
    {
        return await _dbContext.Moods
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Date.Date == date.Date);
    }

    public async Task<List<MoodEntity>> GetMoodsForUserAsync(string userId, DateTime fromDate, DateTime toDate)
    {
        return await _dbContext.Moods
            .Where(m => m.UserId == userId && m.Date.Date >= fromDate.Date && m.Date.Date <= toDate.Date)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<List<MoodEntity>> GetMoodsForUsersAsync(List<string> userIds, DateTime fromDate, DateTime toDate)
    {
        return await _dbContext.Moods
            .Where(m => userIds.Contains(m.UserId) && m.Date.Date >= fromDate.Date && m.Date.Date <= toDate.Date)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<MoodEntity> AddAsync(MoodEntity mood)
    {
        await _dbContext.Moods.AddAsync(mood);
        return mood;
    }

    public Task UpdateAsync(MoodEntity mood)
    {
        _dbContext.Moods.Update(mood);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

