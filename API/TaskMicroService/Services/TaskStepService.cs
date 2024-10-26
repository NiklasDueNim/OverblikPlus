using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
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

        // Get all steps for a specific task by TaskId
        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId)
        {
            var steps = await _dbContext.TaskSteps
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();

            return _mapper.Map<List<ReadTaskStepDto>>(steps);
        }

        // Get a specific step by TaskId and StepNumber
        public async Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepNumber)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            return taskStep != null ? _mapper.Map<ReadTaskStepDto>(taskStep) : null;
        }

        // Create a new TaskStep
        public async Task<int> CreateTaskStep(CreateTaskStepDto createStepDto)
        {
            var taskStep = _mapper.Map<TaskStep>(createStepDto);
            _dbContext.TaskSteps.Add(taskStep);
            await _dbContext.SaveChangesAsync();

            return taskStep.Id;
        }

        // Update an existing TaskStep
        public async Task UpdateTaskStep(int taskId, int stepNumber, UpdateTaskStepDto updateStepDto)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            if (taskStep != null)
            {
                _mapper.Map(updateStepDto, taskStep); // Map updated fields from DTO to the existing entity
                await _dbContext.SaveChangesAsync();
            }
        }

        // Delete a specific TaskStep by TaskId and StepNumber
        public async Task DeleteTaskStep(int taskId, int stepNumber)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            if (taskStep != null)
            {
                _dbContext.TaskSteps.Remove(taskStep);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
