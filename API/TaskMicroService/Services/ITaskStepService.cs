using TaskMicroService.Entities;

namespace TaskMicroService.Services
{
    public interface ITaskStepService
    {
        Task<TaskStep?> GetTaskStep(int taskId, int stepNumber);
        Task<List<TaskStep>> GetAllStepsForTask(int taskId);
        Task<int> CreateTaskStep(TaskStep step);
        Task UpdateTaskStep(int taskId, int stepNumber, TaskStep updatedStep);
        Task DeleteTaskStep(int taskId, int stepNumber);
    }
}