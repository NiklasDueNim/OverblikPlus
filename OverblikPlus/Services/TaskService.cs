using System.Net.Http.Json;
using OverblikPlus.Dtos;
using OverblikPlus.Dtos.Tasks;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<T> ExecuteGetRequest<T>(string uri)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<T>(uri);
            return result ?? throw new InvalidOperationException("No data received.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from {uri}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ReadTaskDto>> GetAllTasks() =>
        await ExecuteGetRequest<List<ReadTaskDto>>("/api/Task");

    public async Task<List<ReadTaskDto>> GetTasksForUserAsync(int userId) =>
        await ExecuteGetRequest<List<ReadTaskDto>>($"api/Task/user/{userId}");
    

    
    public async Task<ReadTaskDto> GetTaskById(int taskId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ReadTaskDto>($"/api/Task/{taskId}")
                   ?? throw new InvalidOperationException("Task not found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching task by ID: {ex.Message}");
            throw;
        }
    }
    
    public async Task<bool> CreateTask(CreateTaskDto newTask)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Task", newTask);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating task: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> UpdateTask(int taskId, UpdateTaskDto updatedTask)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Task/{taskId}", updatedTask);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating task: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> DeleteTask(int taskId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/Task/{taskId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting task: {ex.Message}");
            return false;
        }
    }
}