using TaskMicroService.dto;

namespace TaskMicroService.Services.Interfaces
{
    public interface ITaskStepService
    {
        Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId);
        Task<int> CreateTaskStep(CreateTaskStepDto createStepDto);
        Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepId);
        Task UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updateStepDto); 
        Task DeleteTaskStep(int taskId, int stepId);
    }
}