using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Services
{
    public interface ITaskStepService
    {
        Task<TaskStep?> GetTaskStep(int taskId, int stepNumber);
        Task<List<TaskStep>> GetAllStepsForTask(int taskId);
        Task<int> CreateTaskStep(TaskStep step); // Ændret til TaskStep
        Task UpdateTaskStep(int taskId, int stepNumber, TaskStepDto updatedStepDto);
        Task DeleteTaskStep(int taskId, int stepNumber);
    }
}