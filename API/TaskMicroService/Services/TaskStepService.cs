using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.dto;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;

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

        // Opretter TaskStep baseret på en DTO og mapper til entiteten
        public async Task<int> CreateTaskStep(TaskStepDto stepDto)
        {
            var taskStep = _mapper.Map<TaskStep>(stepDto);
            _dbContext.TaskSteps.Add(taskStep);
            await _dbContext.SaveChangesAsync();
            return taskStep.Id; // Returner den nyoprettede TaskStep Id
        }

        public async Task<TaskStepDto?> GetTaskStep(int taskId, int stepNumber)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(step => step.TaskId == taskId && step.StepNumber == stepNumber);

            return taskStep != null ? _mapper.Map<TaskStepDto>(taskStep) : null;
        }

        public async Task<List<TaskStepDto>> GetAllStepsForTask(int taskId)
        {
            var taskSteps = await _dbContext.TaskSteps
                .Where(step => step.TaskId == taskId)
                .OrderBy(step => step.StepNumber)
                .ToListAsync();
            
            return _mapper.Map<List<TaskStepDto>>(taskSteps);
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
