using System.Net.Http.Json;
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

    private async Task<bool> ExecuteNonQueryRequest(Func<Task<HttpResponseMessage>> requestFunc, string actionDescription)
    {
        try
        {
            var response = await requestFunc();
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error during {actionDescription}. Status Code: {response.StatusCode}, Details: {errorContent}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during {actionDescription}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<ReadTaskDto>> GetAllTasks() =>
        await ExecuteGetRequest<List<ReadTaskDto>>("/api/Task");

    public async Task<List<ReadTaskDto>> GetTasksForUserAsync(int userId) =>
        await ExecuteGetRequest<List<ReadTaskDto>>($"api/Task/user/{userId}");
    
    public async Task<ReadTaskDto> GetTaskById(int taskId) =>
        await ExecuteGetRequest<ReadTaskDto>($"/api/Task/{taskId}");

    public async Task<bool> CreateTask(CreateTaskDto newTask) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.PostAsJsonAsync("/api/Task", newTask),
            "task creation");

    public async Task<bool> UpdateTask(int taskId, UpdateTaskDto updatedTask) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.PutAsJsonAsync($"/api/Task/{taskId}", updatedTask),
            "task update");

    public async Task<bool> DeleteTask(int taskId) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.DeleteAsync($"/api/Task/{taskId}"),
            "task deletion");
}
