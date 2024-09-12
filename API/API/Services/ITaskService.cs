namespace API.Services;

public interface ITaskService
{
    IEnumerable<Task> GetAllTasks();
    Task GetTaskById(int id);
    void CreateTask(Task task);
    void DeleteTask(int id);
    void UpdateTask(Task task);
    
}