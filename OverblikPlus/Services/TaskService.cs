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

    public async Task<List<ReadTaskDto>> GetAllTasks()
    {
        return await _httpClient.GetFromJsonAsync<List<ReadTaskDto>>("/api/Task");
    }

    public async Task<List<ReadTaskDto>> GetTasksForUserAsync(int userId)
    {
        return await _httpClient.GetFromJsonAsync<List<ReadTaskDto>>($"api/Task/user/{userId}");
    }

    public async Task<List<ReadTaskDto>> GetTasksForTodayAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ReadTaskDto>>("api/Task/today");
    }
}