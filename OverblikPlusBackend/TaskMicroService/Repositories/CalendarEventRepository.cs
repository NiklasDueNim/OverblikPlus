using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class CalendarEventRepository : ICalendarEventRepository
{
    private readonly TaskDbContext _dbContext;

    public CalendarEventRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<CalendarEvent>> GetEventsByUserIdAsync(string userId)
    {
        return await _dbContext.CalendarEvents
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<CalendarEvent>> GetEventsByUserIdAndDateAsync(string userId, DateTime date)
    {
        return await _dbContext.CalendarEvents
            .Where(e => e.UserId == userId && e.StartDateTime.Date == date.Date)
            .ToListAsync();
    }

    public async Task<List<CalendarEvent>> GetEventsByUserIdAndDateRangeAsync(string userId, DateTime from, DateTime to)
    {
        return await _dbContext.CalendarEvents
            .Where(e => e.UserId == userId && e.StartDateTime >= from && e.StartDateTime <= to)
            .ToListAsync();
    }

    public async Task<CalendarEvent?> GetByIdAsync(Guid id)
    {
        return await _dbContext.CalendarEvents.FindAsync(id);
    }

    public async Task<CalendarEvent> AddAsync(CalendarEvent calendarEvent)
    {
        await _dbContext.CalendarEvents.AddAsync(calendarEvent);
        return calendarEvent;
    }

    public Task UpdateAsync(CalendarEvent calendarEvent)
    {
        _dbContext.CalendarEvents.Update(calendarEvent);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CalendarEvent calendarEvent)
    {
        _dbContext.CalendarEvents.Remove(calendarEvent);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

