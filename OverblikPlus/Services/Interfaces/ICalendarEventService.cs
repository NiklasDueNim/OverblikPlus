using OverblikPlus.Models.Dtos.Calendar;

namespace OverblikPlus.Services.Interfaces;

public interface ICalendarEventService
{
    Task<ReadCalendarEventDto?> GetEventByIdAsync(int id);
    Task<IEnumerable<ReadCalendarEventDto>> GetAllEventsAsync(string userId);
    Task CreateEventAsync(CreateCalendarEventDto dto);
    Task UpdateEventAsync(int id, CreateCalendarEventDto dto);
    Task DeleteEventAsync(int id);
}