using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class ShiftRepository : IShiftRepository
{
    private readonly TaskDbContext _dbContext;

    public ShiftRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<ShiftEntity>> GetShiftsForDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbContext.Shifts
            .Where(s => s.StartTime.Date >= fromDate.Date && s.StartTime.Date <= toDate.Date)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<ShiftEntity>> GetShiftsForUserAsync(string userId, DateTime fromDate, DateTime toDate)
    {
        return await _dbContext.Shifts
            .Where(s => s.UserId == userId && s.StartTime.Date >= fromDate.Date && s.StartTime.Date <= toDate.Date)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<ShiftEntity?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Shifts.FindAsync(id);
    }

    public async Task<ShiftEntity> AddAsync(ShiftEntity shift)
    {
        await _dbContext.Shifts.AddAsync(shift);
        return shift;
    }

    public Task DeleteAsync(ShiftEntity shift)
    {
        _dbContext.Shifts.Remove(shift);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

