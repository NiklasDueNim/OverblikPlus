using DataAccess;

namespace API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public IEnumerable<Task> GetAllTasks()
    {
        throw new NotImplementedException();
    }

    public Task GetTaskById(int id)
    {
        return _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
    }

    public void CreateTask(Task task)
    {
        throw new NotImplementedException();
    }

    public void DeleteTask(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateTask(Task task)
    {
        throw new NotImplementedException();
    }
}