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

        // Get all steps for a specific task by TaskId
        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId)
        {
            var steps = await _dbContext.TaskSteps
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();

            // Mapper TaskStep til ReadTaskStepDto
            var stepDtos = _mapper.Map<List<ReadTaskStepDto>>(steps);

            // Konvertere hver TaskStep's ImageUrl (byte[]) til en Base64-string og sæt den til Image-feltet i ReadTaskStepDto
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

            // Konverter base64-streng til byte[], før du gemmer, hvis billedet er inkluderet
            if (!string.IsNullOrEmpty(createStepDto.ImageBase64))
            {
                taskStep.ImageUrl = Convert.FromBase64String(createStepDto.ImageBase64); // Brug ImageUrl her
            }

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
