using TaskMicroService.dto;

namespace TaskMicroService.Services
{
    public interface ITaskStepService
    {
        Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId);
        Task<int> CreateTaskStep(CreateTaskStepDto createStepDto);
        Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepNumber);
        Task UpdateTaskStep(int taskId, int stepNumber, UpdateTaskStepDto updateStepDto); 
        Task DeleteTaskStep(int taskId, int stepNumber);
    }
}