using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface ITaskStepRepository
{
    Task<List<TaskStep>> GetStepsForTaskAsync(int taskId);
    Task<TaskStep?> GetTaskStepAsync(int taskId, int stepId);
    Task<TaskStep> AddAsync(TaskStep taskStep);
    Task UpdateAsync(TaskStep taskStep);
    Task DeleteAsync(TaskStep taskStep);
    Task SaveChangesAsync();
}

