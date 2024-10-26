using TaskMicroService.dto;

namespace TaskMicroService.Services
{
    public interface ITaskStepService
    {
        Task<TaskStepDto?> GetTaskStep(int taskId, int stepNumber);
        Task<List<TaskStepDto>> GetAllStepsForTask(int taskId);
        Task<int> CreateTaskStep(TaskStepDto stepDto); // Ændret til at tage TaskStepDto som parameter
        Task UpdateTaskStep(int taskId, int stepNumber, TaskStepDto updatedStepDto);
        Task DeleteTaskStep(int taskId, int stepNumber);
    }
}