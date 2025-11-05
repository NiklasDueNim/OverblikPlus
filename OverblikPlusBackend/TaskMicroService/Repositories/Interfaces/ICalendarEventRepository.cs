using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByUserIdAsync(string userId);
    Task<List<CalendarEvent>> GetEventsByUserIdAndDateAsync(string userId, DateTime date);
    Task<List<CalendarEvent>> GetEventsByUserIdAndDateRangeAsync(string userId, DateTime from, DateTime to);
    Task<CalendarEvent?> GetByIdAsync(Guid id);
    Task<CalendarEvent> AddAsync(CalendarEvent calendarEvent);
    Task UpdateAsync(CalendarEvent calendarEvent);
    Task DeleteAsync(CalendarEvent calendarEvent);
    Task SaveChangesAsync();
}

