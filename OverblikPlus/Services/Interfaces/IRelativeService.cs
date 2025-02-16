using OverblikPlus.Models.Dtos.Calendar;
using OverblikPlus.Models.Dtos.Tasks;

namespace OverblikPlus.Services.Interfaces;

public interface IRelativeService
{
    Task<IEnumerable<ReadTaskDto>> GetTasksForDayForSpecificUser(string userId, DateTime date);
    Task<IEnumerable<ReadCalendarEventDto>> GetEventsForDayForSpecificUser(string userId, DateTime date);

}