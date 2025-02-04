using TaskMicroService.dtos.Task;

namespace TaskMicroService.Services.Interfaces;

public interface IRelativeService
{
    Task<IEnumerable<ReadTaskDto>> GetTasksForDayForSpecificUser(string userId, DateTime date);
}