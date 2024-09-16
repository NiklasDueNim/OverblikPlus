using API.Dto;
using DataAccess;
using DataAccess.Models;

namespace API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public IEnumerable<TaskDto> GetAllTasks()
    {
        throw new NotImplementedException();
    }

    public TaskDto GetTaskById(int id)
    {
        var task = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
        return TaskMapper.MapToTaskDto(task);
    }

    public void CreateTask(TaskDto taskDto)
    {
        throw new NotImplementedException();
    }

    public void DeleteTask(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateTask(TaskDto task)
    {
        throw new NotImplementedException();
    }
}