using TaskMicroService.Common;
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
        Task<Result> MarkTaskAsCompleted(int taskId);
        Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date);
    }
}