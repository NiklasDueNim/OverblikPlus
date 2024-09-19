using API.Dto;
using DataAccess.Models;

namespace API.Services
{
    public interface ITaskService
    {
        IEnumerable<TaskDto> GetAllTasks();
        TaskDto GetTaskById(int id);
        int CreateTask(CreateTaskDto createTaskDto);
        void DeleteTask(int id);
        void UpdateTask(TaskDto taskDto);
    
    }
}

