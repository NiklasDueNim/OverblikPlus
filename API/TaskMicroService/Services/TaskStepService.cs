using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.Entities;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;

namespace TaskMicroService.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly TaskDbContext _dbContext;
        private readonly IMapper _mapper;

        public TaskStepService(TaskDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<int> CreateTaskStep(TaskStep step)
        {
            _dbContext.TaskSteps.Add(step);
            await _dbContext.SaveChangesAsync();
            return step.StepNumber;
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

        public async Task UpdateTaskStep(int taskId, int stepNumber, TaskStepDto updatedStepDto)
        {
            var existingStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(step => step.TaskId == taskId && step.StepNumber == stepNumber);

            if (existingStep != null)
            {
                _mapper.Map(updatedStepDto, existingStep);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteTaskStep(int taskId, int stepNumber)
        {
            var step = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            if (step != null)
            {
                _dbContext.TaskSteps.Remove(step);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
