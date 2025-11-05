using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByUserIdAsync(string userId);
    Task<CalendarEvent?> GetByIdAsync(Guid id);
    Task<CalendarEvent> AddAsync(CalendarEvent calendarEvent);
    Task UpdateAsync(CalendarEvent calendarEvent);
    Task DeleteAsync(CalendarEvent calendarEvent);
    Task SaveChangesAsync();
}

