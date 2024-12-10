using OverblikPlus.Dtos.Tasks;

namespace OverblikPlus.Services.Interfaces
{
    public interface ITaskService
    {
        Task<List<ReadTaskDto>> GetAllTasks();
        Task<List<ReadTaskDto>> GetTasksForUserAsync(string userId);
        
        Task<ReadTaskDto> GetTaskById(int taskId);
        
        Task<bool> CreateTask(CreateTaskDto newTask);
        
        Task<bool> UpdateTask(int taskId, UpdateTaskDto updatedTask);
        
        Task<bool> DeleteTask(int taskId);
        Task MarkTaskAsCompleted(int taskId);
        Task<List<ReadTaskDto>> GetTasksForDay(string userId, DateTime date);
    }
}