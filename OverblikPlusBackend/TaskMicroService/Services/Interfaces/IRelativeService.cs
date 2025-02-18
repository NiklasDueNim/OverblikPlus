using Microsoft.ApplicationInsights.Extensibility.Implementation;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;

namespace TaskMicroService.Services.Interfaces;

public interface IRelativeService
{
    Task<IEnumerable<ReadTaskDto>> GetTasksForDayForSpecificUser(string userId, DateTime date);
    Task<IEnumerable<ReadCalendarEventDto>> GetEventsForDayForSpecificUser(string userId, DateTime date);
    
    Task<IEnumerable<ReadCalendarEventDto>> GetEventsForIntervalForUser(string userId, DateTime startDate, DateTime endDate);
}