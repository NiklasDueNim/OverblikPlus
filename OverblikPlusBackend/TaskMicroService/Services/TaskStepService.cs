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
        private readonly IImageConversionService _imageConversionService;

        public TaskStepService(TaskDbContext dbContext, IMapper mapper, IImageConversionService imageConversionService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _imageConversionService = imageConversionService;
        }
        
        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId)
        {
            var steps = await _dbContext.TaskSteps
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();

            var stepDtos = _mapper.Map<List<ReadTaskStepDto>>(steps);

            foreach (var stepDto in stepDtos)
            {
                var originalStep = steps.FirstOrDefault(s => s.Id == stepDto.Id);
                if (originalStep?.ImageUrl != null)
                {
                    stepDto.Image = _imageConversionService.ConvertToBase64(originalStep.ImageUrl);
                }
            }

            return stepDtos;
        }

        
        public async Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepId)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            return taskStep != null ? _mapper.Map<ReadTaskStepDto>(taskStep) : null;
        }
        
        public async Task<int> CreateTaskStep(CreateTaskStepDto createStepDto)
        {
            var taskStep = _mapper.Map<TaskStep>(createStepDto);
            
            if (!string.IsNullOrEmpty(createStepDto.ImageBase64))
            {
                taskStep.ImageUrl = Convert.FromBase64String(createStepDto.ImageBase64);
            }

            _dbContext.TaskSteps.Add(taskStep);
            await _dbContext.SaveChangesAsync();

            return taskStep.Id;
        }

        
        public async Task UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updateStepDto)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            if (taskStep != null)
            {
                _mapper.Map(updateStepDto, taskStep); 
                await _dbContext.SaveChangesAsync();
            }
        }


        public async Task DeleteTaskStep(int taskId, int stepId)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            if (taskStep != null)
            {
                _dbContext.TaskSteps.Remove(taskStep);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
