using OverblikPlus.Pages;

namespace OverblikPlus.Services;

public interface ITaskService
{
    Task<List<TaskList.TaskItem>> GetTasksAsync();
    Task MarkTaskAsCompleted(int taskId);

}