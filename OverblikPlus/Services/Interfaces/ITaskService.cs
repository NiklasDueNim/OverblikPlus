using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Tasks;

namespace OverblikPlus.Services.Interfaces
{
    public interface ITaskService
    {
        Task<Result<List<ReadTaskDto>>> GetAllTasks();
        Task<Result<List<ReadTaskDto>>> GetTasksForUserAsync(string userId);
        
        Task<Result<ReadTaskDto>> GetTaskById(int taskId);
        
        Task<Result<int>> CreateTask(CreateTaskDto newTask);
        
        Task<Result> UpdateTask(int taskId, UpdateTaskDto updatedTask);
        
        Task<Result> DeleteTask(int taskId);
        Task<Result> MarkTaskAsCompleted(int taskId);
        Task<Result<List<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date);
    }
}