using System.Net.Http.Json;
using OverblikPlus.Dtos.Tasks;
using OverblikPlus.Services.Interfaces;
using OverblikPlus.Common;

namespace OverblikPlus.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // Generic method for executing GET requests with Result<T>
    private async Task<Result<T>> ExecuteGetRequest<T>(string uri)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Result<T>>(uri);
            return response ?? Result<T>.ErrorResult("No data received.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from {uri}: {ex.Message}");
            return Result<T>.ErrorResult($"Exception occurred: {ex.Message}");
        }
    }

    // Generic method for non-query requests (POST, PUT, DELETE)
    private async Task<Result> ExecuteNonQueryRequest(Func<Task<HttpResponseMessage>> requestFunc, string actionDescription)
    {
        try
        {
            var response = await requestFunc();
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error during {actionDescription}. Status: {response.StatusCode}, Details: {errorContent}");
                return Result.ErrorResult($"{actionDescription} failed: {errorContent}");
            }
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during {actionDescription}: {ex.Message}");
            return Result.ErrorResult($"Exception occurred: {ex.Message}");
        }
    }

    // Fetch all tasks
    public async Task<Result<List<ReadTaskDto>>> GetAllTasks() =>
        await ExecuteGetRequest<List<ReadTaskDto>>("/api/Task");

    // Fetch tasks for a specific user
    public async Task<Result<List<ReadTaskDto>>> GetTasksForUserAsync(string userId) =>
        await ExecuteGetRequest<List<ReadTaskDto>>($"api/Task/user/{userId}");

    // Fetch task by ID
    public async Task<Result<ReadTaskDto>> GetTaskById(int taskId) =>
        await ExecuteGetRequest<ReadTaskDto>($"/api/Task/{taskId}");

    // Create a new task
    public async Task<Result<int>> CreateTask(CreateTaskDto newTask)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Task", newTask);
        if (response.IsSuccessStatusCode)
        {
            var createdTaskId = await response.Content.ReadFromJsonAsync<int>();
            return Result<int>.SuccessResult(createdTaskId);
        }
        var errorContent = await response.Content.ReadAsStringAsync();
        return Result<int>.ErrorResult($"Failed to create task. Details: {errorContent}");
    }

    // Update an existing task
    public async Task<Result> UpdateTask(int taskId, UpdateTaskDto updatedTask) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.PutAsJsonAsync($"/api/Task/{taskId}", updatedTask),
            "Task update");

    // Delete a task
    public async Task<Result> DeleteTask(int taskId) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.DeleteAsync($"/api/Task/{taskId}"),
            "Task deletion");

    // Fetch tasks for the current user
    public async Task<Result<List<ReadTaskDto>>> GetTasksForCurrentUserAsync() =>
        await ExecuteGetRequest<List<ReadTaskDto>>("api/Task/user-tasks");

    // Mark task as completed
    public async Task<Result> MarkTaskAsCompleted(int taskId)
    {
        return await ExecuteNonQueryRequest(
            () => _httpClient.PutAsync($"/api/task/{taskId}/complete", null),
            "Mark task as completed");
    }

    // Fetch tasks for a specific day
    public async Task<Result<List<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date)
    {
        return await ExecuteGetRequest<List<ReadTaskDto>>($"api/Task/user/{userId}/tasks-for-day?date={date:yyyy-MM-dd}");
    }
}