using TaskMicroService.Common;
using TaskMicroService.Dtos.Calendar;

namespace TaskMicroService.Services.Interfaces;

public interface ICalendarEventService
{
    Task<Result<IEnumerable<ReadCalendarEventDto>>> GetAllEventsAsync(string userId);
    
    Task<Result<ReadCalendarEventDto?>> GetEventByIdAsync(Guid id);
    
    Task<Result<ReadCalendarEventDto>> CreateEventAsync(CreateCalendarEventDto dto);
    
    Task<Result<bool>> UpdateEventAsync(Guid id, CreateCalendarEventDto dto);
    
    Task<Result<bool>> DeleteEventAsync(Guid id);
}