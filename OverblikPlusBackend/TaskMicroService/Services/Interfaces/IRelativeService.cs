using OverblikPlus.Shared.Common;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;

namespace TaskMicroService.Services.Interfaces;

public interface IRelativeService
{
    Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDayForSpecificUser(string userId, DateTime date);
    Task<Result<IEnumerable<ReadCalendarEventDto>>> GetEventsForDayForSpecificUser(string userId, DateTime date);
    Task<Result<IEnumerable<ReadCalendarEventDto>>> GetEventsForIntervalForUser(string userId, DateTime startDate, DateTime endDate);
}