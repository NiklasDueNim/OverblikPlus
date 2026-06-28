using OverblikPlus.Shared.Common;
using TaskMicroService.dtos.Task;

namespace TaskMicroService.Services.Interfaces
{
    public interface ITaskService
    {
        Task<Result<IEnumerable<ReadTaskDto>>> GetAllTasks();
        Task<Result<ReadTaskDto>> GetTaskById(int id);
        Task<Result<IEnumerable<ReadTaskDto>>> GetTasksByUserId(string userId);
        Task<Result<int>> CreateTask(CreateTaskDto createTaskDto);
        Task<Result> DeleteTask(int id);
        Task<Result> UpdateTask(int id, UpdateTaskDto updateTaskDto);
        Task<Result> MarkTaskAsCompleted(int taskId, DateTime occurrenceDate);
        Task<Result> MarkTaskAsUnCompleted(int taskId, DateTime occurrenceDate);
        Task<Result<IEnumerable<TaskCompletionDto>>> GetCompletions(string userId, DateTime from, DateTime to);
        Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date);
    }
}