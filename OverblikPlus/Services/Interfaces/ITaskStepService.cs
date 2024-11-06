using OverblikPlus.Dtos.TaskSteps;

namespace OverblikPlus.Services.Interfaces
{
    public interface ITaskStepService
    {
        Task<List<TaskStepDto>> GetStepsForTask(int taskId);
        Task<TaskStepDto?> GetTaskStep(int taskId, int stepId);
        Task<bool> CreateTaskStep(CreateTaskStepDto newStep);
        Task<bool> UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updatedStep);
        Task<bool> DeleteTaskStep(int taskId, int stepId);
    }
}