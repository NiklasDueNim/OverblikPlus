using System.Net.Http.Json;
using OverblikPlus;
using OverblikPlus.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<List<TaskDto>> GetAllTasksAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<TaskDto>> GetTasksForUserAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<TaskDto>> GetTasksForTodayAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TaskDto>>("api/tasks/today");
    }
}