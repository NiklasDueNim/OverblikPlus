using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class TaskStepRepository : ITaskStepRepository
{
    private readonly TaskDbContext _dbContext;

    public TaskStepRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<TaskStep>> GetStepsForTaskAsync(int taskId)
    {
        return await _dbContext.TaskSteps
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();
    }

    public async Task<TaskStep?> GetTaskStepAsync(int taskId, int stepId)
    {
        return await _dbContext.TaskSteps
            .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);
    }

    public async Task<TaskStep> AddAsync(TaskStep taskStep)
    {
        await _dbContext.TaskSteps.AddAsync(taskStep);
        return taskStep;
    }

    public Task UpdateAsync(TaskStep taskStep)
    {
        _dbContext.TaskSteps.Update(taskStep);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskStep taskStep)
    {
        _dbContext.TaskSteps.Remove(taskStep);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

