using OverblikPlus.Models.Dtos.Calendar;

namespace OverblikPlus.Services.Interfaces;

public interface ICalendarEventService
{
    Task<ReadCalendarEventDto?> GetEventByIdAsync(Guid id);
    Task<IEnumerable<ReadCalendarEventDto>> GetAllEventsAsync(string userId);
    Task CreateEventAsync(CreateCalendarEventDto dto);
    Task UpdateEventAsync(Guid id, CreateCalendarEventDto dto);
    Task DeleteEventAsync(Guid id);
}