using TaskMicroService.dto;

namespace TaskMicroService.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<ReadTaskDto>> GetAllTasks();
        Task<ReadTaskDto> GetTaskById(int id);
        Task<int> CreateTask(CreateTaskDto createTaskDto);
        Task DeleteTask(int id);
        Task UpdateTask(int id, UpdateTaskDto updateTaskDto);
    }
}