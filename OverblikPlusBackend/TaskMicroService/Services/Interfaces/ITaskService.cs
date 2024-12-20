using TaskMicroService.dto;

namespace TaskMicroService.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<ReadTaskDto>> GetAllTasks();
        Task<ReadTaskDto> GetTaskById(int id);

        Task<IEnumerable<ReadTaskDto>> GetTasksByUserId(string userId);
        Task<int> CreateTask(CreateTaskDto createTaskDto);
        Task DeleteTask(int id);
        Task UpdateTask(int id, UpdateTaskDto updateTaskDto);
        Task MarkTaskAsCompleted(int taskId);
        Task<IEnumerable<ReadTaskDto>> GetTasksForDay(string userId, DateTime date);
    }
}