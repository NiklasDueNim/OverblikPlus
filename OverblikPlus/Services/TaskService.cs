using System.Net.Http.Json;
using Microsoft.VisualBasic;
using OverblikPlus.Pages;

namespace OverblikPlus.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<TaskList.TaskItem>> GetTasksAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TaskList.TaskItem>>("api/task/tasks");
    }

    public async Task MarkTaskAsCompleted(int taskId)
    {
        throw new NotImplementedException();
    }
}