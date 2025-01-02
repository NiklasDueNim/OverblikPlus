using System.Net.Http.Json;
using OverblikPlus.Dtos.TaskSteps;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly HttpClient _httpClient;

        public TaskStepService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Console.WriteLine($"TaskStepService BaseAddress: {_httpClient.BaseAddress}");
        }

        private async Task<T?> ExecuteGetRequest<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<T>(url);
                if (response == null)
                {
                    throw new Exception("No data received.");
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during GET request to {url}: {ex.Message}");
                return default;
            }
        }

        private async Task<bool> ExecuteNonQueryRequest(Func<Task<HttpResponseMessage>> httpRequest, string errorMessage)
        {
            try
            {
                var response = await httpRequest();
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"{errorMessage}. Status Code: {response.StatusCode}, Details: {errorContent}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{errorMessage}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId) =>
            await ExecuteGetRequest<List<ReadTaskStepDto>>($"/api/tasks/{taskId}/steps") ?? new List<ReadTaskStepDto>();

        public async Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepId) =>
            await ExecuteGetRequest<ReadTaskStepDto>($"/api/tasks/{taskId}/steps/{stepId}");

        public async Task<bool> CreateTaskStep(CreateTaskStepDto newStep)
        {
            return await ExecuteNonQueryRequest(
                () => _httpClient.PostAsJsonAsync($"/api/tasks/{newStep.TaskId}/steps", newStep),
                $"Error creating step for task {newStep.TaskId}"
            );
        }

        public async Task<bool> UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updatedStep)
        {
            return await ExecuteNonQueryRequest(
                () => _httpClient.PutAsJsonAsync($"/api/tasks/{taskId}/steps/{stepId}", updatedStep),
                $"Error updating step {stepId} for task {taskId}"
            );
        }

        public async Task<bool> DeleteTaskStep(int taskId, int stepId)
        {
            return await ExecuteNonQueryRequest(
                () => _httpClient.DeleteAsync($"/api/tasks/{taskId}/steps/{stepId}"),
                $"Error deleting step {stepId} for task {taskId}"
            );
        }
    }
}