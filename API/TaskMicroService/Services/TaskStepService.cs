using Microsoft.EntityFrameworkCore;
using TaskMicroService.Entities;
using TaskMicroService.DataAccess;

namespace TaskMicroService.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly TaskDbContext _dbContext;

        public TaskStepService(TaskDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskStep?> GetTaskStep(int taskId, int stepNumber)
        {
            return await _dbContext.TaskSteps
                .FirstOrDefaultAsync(step => step.TaskId == taskId && step.StepNumber == stepNumber);
        }

        public async Task<List<TaskStep>> GetAllStepsForTask(int taskId)
        {
            return await _dbContext.TaskSteps
                .Where(step => step.TaskId == taskId)
                .OrderBy(step => step.StepNumber)
                .ToListAsync();
        }
        
        public async Task<int> CreateTaskStep(TaskStep step)
        {
            // Fjern eventuelle værdier for Id, så databasen kan generere det automatisk
            _dbContext.TaskSteps.Add(step);
            await _dbContext.SaveChangesAsync();
            return step.Id;
        }


        public async Task UpdateTaskStep(int taskId, int stepNumber, TaskStep updatedStep)
        {
            var existingStep = await GetTaskStep(taskId, stepNumber);
            if (existingStep != null)
            {
                existingStep.ImageUrl = updatedStep.ImageUrl;
                existingStep.Text = updatedStep.Text;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteTaskStep(int taskId, int stepNumber)
        {
            var step = await GetTaskStep(taskId, stepNumber);
            if (step != null)
            {
                _dbContext.TaskSteps.Remove(step);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}