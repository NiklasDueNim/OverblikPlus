using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OverblikPlus.AuthHelpers;
using OverblikPlus.Services.Interfaces;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Tasks;


namespace OverblikPlus.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly ILogger<TaskService> _logger;

    public TaskService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILogger<TaskService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task<Result<T>> ExecuteGetRequest<T>(string uri)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Result<T>>(uri);
            return response ?? Result<T>.ErrorResult("No data received.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from {Uri}", uri);
            return Result<T>.ErrorResult($"Exception occurred: {ex.Message}");
        }
    }

    private async Task<Result> ExecuteNonQueryRequest(Func<Task<HttpResponseMessage>> requestFunc, string actionDescription)
    {
        try
        {
            var response = await requestFunc();
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error during {Action}. Status: {StatusCode}, Details: {ErrorContent}", 
                    actionDescription, response.StatusCode, errorContent);
                return Result.ErrorResult($"{actionDescription} failed: {errorContent}");
            }
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during {Action}", actionDescription);
            return Result.ErrorResult($"Exception occurred: {ex.Message}");
        }
    }

    public async Task<Result<List<ReadTaskDto>>> GetAllTasks() =>
        await ExecuteGetRequest<List<ReadTaskDto>>("/api/Task");

    public async Task<Result<List<ReadTaskDto>>> GetTasksForUserAsync(string userId) =>
        await ExecuteGetRequest<List<ReadTaskDto>>($"/api/Task/user/{userId}");

    public async Task<Result<ReadTaskDto>> GetTaskById(int taskId) =>
        await ExecuteGetRequest<ReadTaskDto>($"/api/Task/{taskId}");

    public async Task<Result<int>> CreateTask(CreateTaskDto newTask)
    {
        try
        {
            var userId = _authStateProvider.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId could not be retrieved from the token.");
                return Result<int>.ErrorResult("UserId is required for the task.");
            }
            
            newTask.UserId = userId;

            _logger.LogDebug("Sending request to API: {TaskData}", JsonConvert.SerializeObject(newTask));

            var response = await _httpClient.PostAsJsonAsync("/api/Task", newTask);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("API Response: {Response}", jsonResponse);

            var apiResponse = JsonConvert.DeserializeObject<Result<int>>(jsonResponse);

            if (apiResponse?.Success == true)
            {
                _logger.LogInformation("Task created successfully with ID: {TaskId}", apiResponse.Data);
                return Result<int>.SuccessResult(apiResponse.Data);
            }
            return Result<int>.ErrorResult($"Failed to create task. Details: {apiResponse?.Error}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateTask");
            return Result<int>.ErrorResult($"Exception: {ex.Message}");
        }
    }


    public async Task<Result> UpdateTask(int taskId, UpdateTaskDto updatedTask) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.PutAsJsonAsync($"/api/Task/{taskId}", updatedTask),
            "Task update");

    public async Task<Result> DeleteTask(int taskId) =>
        await ExecuteNonQueryRequest(
            () => _httpClient.DeleteAsync($"/api/Task/{taskId}"),
            "Task deletion");

    public async Task<Result<List<ReadTaskDto>>> GetTasksForCurrentUserAsync() =>
        await ExecuteGetRequest<List<ReadTaskDto>>("/api/Task/user-tasks");

    public async Task<Result> MarkTaskAsCompleted(int taskId, DateTime occurrenceDate)
    {
        _logger.LogDebug("Calling MarkTaskAsCompleted API for task {TaskId} on {Date}", taskId, occurrenceDate);
        var result = await ExecuteNonQueryRequest(
            () => _httpClient.PutAsync($"/api/Task/{taskId}/complete?date={occurrenceDate:yyyy-MM-dd}", null),
            "Mark task as completed");
        if (!result.Success)
        {
            _logger.LogWarning("Failed to mark task {TaskId} as completed: {Error}", taskId, result.Error);
        }
        return result;
    }

    public async Task<Result> MarkTaskAsUnCompleted(int taskId, DateTime occurrenceDate)
    {
        _logger.LogDebug("Calling MarkTaskAsUnCompleted API for task {TaskId} on {Date}", taskId, occurrenceDate);
        var result = await ExecuteNonQueryRequest(
            () => _httpClient.PutAsync($"/api/Task/{taskId}/notComplete?date={occurrenceDate:yyyy-MM-dd}", null),
            "Mark task as uncompleted");
        if (!result.Success)
        {
            _logger.LogWarning("Failed to mark task {TaskId} as uncompleted: {Error}", taskId, result.Error);
        }
        return result;
    }

    public async Task<Result<List<TaskCompletionDto>>> GetCompletions(string userId, DateTime from, DateTime to) =>
        await ExecuteGetRequest<List<TaskCompletionDto>>(
            $"/api/Task/user/{userId}/completions?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");


    public async Task<Result<List<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date)
    {
        return await ExecuteGetRequest<List<ReadTaskDto>>($"/api/Task/user/{userId}/tasks-for-day?date={date:yyyy-MM-dd}");
    }
}