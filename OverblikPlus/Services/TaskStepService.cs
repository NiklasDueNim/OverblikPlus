using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.TaskSteps;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TaskStepService> _logger;

        public TaskStepService(HttpClient httpClient, ILogger<TaskStepService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<Result<T>> ExecuteGetRequest<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Result<T>>(url);
                if (response != null && response.Success)
                {
                    return response;
                }
                return Result<T>.ErrorResult(response?.Error ?? "No data received.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GET request to {Url}", url);
                return Result<T>.ErrorResult($"Exception occurred: {ex.Message}");
            }
        }

        private async Task<Result> ExecuteNonQueryRequest(Func<Task<HttpResponseMessage>> httpRequest, string actionDescription)
        {
            try
            {
                var response = await httpRequest();
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

        public async Task<Result<List<ReadTaskStepDto>>> GetStepsForTask(int taskId)
        {
            var result = await ExecuteGetRequest<List<ReadTaskStepDto>>($"/api/tasks/{taskId}/steps");
            if (result.Success && result.Data != null)
            {
                return result;
            }
            return Result<List<ReadTaskStepDto>>.SuccessResult(new List<ReadTaskStepDto>());
        }

        public async Task<Result<ReadTaskStepDto>> GetTaskStep(int taskId, int stepId)
        {
            return await ExecuteGetRequest<ReadTaskStepDto>($"/api/tasks/{taskId}/steps/{stepId}");
        }

        public async Task<Result> CreateTaskStep(CreateTaskStepDto newStep)
        {
            return await ExecuteNonQueryRequest(
                () => _httpClient.PostAsJsonAsync($"/api/tasks/{newStep.TaskId}/steps", newStep),
                $"Error creating step for task {newStep.TaskId}"
            );
        }

        public async Task<Result> UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updatedStep)
        {
            return await ExecuteNonQueryRequest(
                () => _httpClient.PutAsJsonAsync($"/api/tasks/{taskId}/steps/{stepId}", updatedStep),
                $"Error updating step {stepId} for task {taskId}"
            );
        }

        public async Task<Result> DeleteTaskStep(int taskId, int stepId)
        {
            return await ExecuteNonQueryRequest(
                () => _httpClient.DeleteAsync($"/api/tasks/{taskId}/steps/{stepId}"),
                $"Error deleting step {stepId} for task {taskId}"
            );
        }
    }
}