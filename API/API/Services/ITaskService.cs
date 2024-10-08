using API.Dto;
using DataAccess.Models;

namespace API.Services
{
    public interface ITaskService
    {
        IEnumerable<ReadTaskDto> GetAllTasks();
        ReadTaskDto GetTaskById(int id);
        int CreateTask(CreateTaskDto createTaskDto);
        void DeleteTask(int id);
        void UpdateTask(int id, UpdateTaskDto updateTaskDto);
    
    }
}

