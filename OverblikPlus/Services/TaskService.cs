using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using OverblikPlus.Dtos.Tasks;
using OverblikPlus.Services.Interfaces;
using OverblikPlus.Common;



namespace OverblikPlus.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;

    public TaskService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
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
            Console.WriteLine($"Error fetching data from {uri}: {ex.Message}");
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
            var userId = _authStateProvider.GetUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("UserId could not be retrieved from the token.");
                return Result<int>.ErrorResult("UserId is required for the task.");
            }
            
            newTask.UserId = userId;

            Console.WriteLine($"Sending request to API: {JsonConvert.SerializeObject(newTask)}");

            var response = await _httpClient.PostAsJsonAsync("/api/Task", newTask);
            Console.WriteLine($"API Response Content: {response}");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"JSON Response: {jsonResponse}");

            var apiResponse = JsonConvert.DeserializeObject<Result<int>>(jsonResponse);

            if (apiResponse.Success)
            {
                return Result<int>.SuccessResult(apiResponse.Data);
            }
            return Result<int>.ErrorResult($"Failed to create task. Details: {apiResponse.Error}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateTask: {ex.Message}");
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

    public async Task<Result> MarkTaskAsCompleted(int taskId)
    {
        return await ExecuteNonQueryRequest(
            () => _httpClient.PutAsync($"/api/task/{taskId}/complete", null),
            "Mark task as completed");
    }

    public async Task<Result<List<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date)
    {
        return await ExecuteGetRequest<List<ReadTaskDto>>($"/api/Task/user/{userId}/tasks-for-day?date={date:yyyy-MM-dd}");
    }
}