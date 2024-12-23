using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly TaskDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<TaskStepService> _logger;

        public TaskStepService(
            TaskDbContext dbContext, 
            IMapper mapper, 
            IBlobStorageService blobStorageService,
            ILogger<TaskStepService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
        }
        
        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId)
        {
            _logger.LogInformation($"Fetching steps for task ID {taskId}");

            var steps = await _dbContext.TaskSteps
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();
            
            if (!steps.Any())
            {
                _logger.LogWarning($"No steps found for task ID {taskId}");
                return new List<ReadTaskStepDto>();
            }

            var stepDtos = _mapper.Map<List<ReadTaskStepDto>>(steps);
            for (int i = 0; i < stepDtos.Count; i++)
            {
                var stepDto = stepDtos[i];
                var originalStep = steps[i];

                stepDto.Image = originalStep.ImageUrl;
                stepDto.NextStepId = (i < stepDtos.Count - 1) ? stepDtos[i + 1].Id : null;
            }

            return stepDtos;
        }

        public async Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepId)
        {
            _logger.LogInformation($"Fetching step ID {stepId} for task ID {taskId}");

            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            if (taskStep == null)
            {
                _logger.LogWarning($"Step ID {stepId} not found for task ID {taskId}");
                return null;
            }

            var stepDto = _mapper.Map<ReadTaskStepDto>(taskStep);
            stepDto.Image = taskStep.ImageUrl;
            return stepDto;
        }
        
        public async Task<int> CreateTaskStep(CreateTaskStepDto createStepDto)
        {
            _logger.LogInformation($"Creating a new step for task ID {createStepDto.TaskId}");

            var taskStep = _mapper.Map<TaskStep>(createStepDto);
            
            if (!string.IsNullOrEmpty(createStepDto.ImageBase64))
            {
                var imageBytes = Convert.FromBase64String(createStepDto.ImageBase64);
                using var stream = new MemoryStream(imageBytes);
                var blobFileName = $"{Guid.NewGuid()}.jpg";

                taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                _logger.LogInformation($"Uploaded image for step ID {taskStep.Id}");
            }
            
            _dbContext.TaskSteps.Add(taskStep);
            await _dbContext.SaveChangesAsync();

            return taskStep.Id;
        }


        public async Task UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updateStepDto)
        {
            _logger.LogInformation($"Updating step ID {stepId} for task ID {taskId}");

            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            if (taskStep == null)
            {
                _logger.LogWarning($"Step ID {stepId} not found for task ID {taskId}");
                throw new KeyNotFoundException($"Step ID {stepId} not found for task ID {taskId}");
            }

            _mapper.Map(updateStepDto, taskStep);

            if (!string.IsNullOrEmpty(updateStepDto.ImageBase64))
            {
                if (!string.IsNullOrEmpty(taskStep.ImageUrl))
                {
                    var oldBlobFileName = taskStep.ImageUrl.Split('/').Last();
                    await _blobStorageService.DeleteImageAsync(oldBlobFileName);
                    _logger.LogInformation($"Deleted old image for step ID {stepId}");
                }
                
                var imageBytes = Convert.FromBase64String(updateStepDto.ImageBase64);
                using var stream = new MemoryStream(imageBytes);
                var blobFileName = $"{Guid.NewGuid()}.jpg";
                taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                _logger.LogInformation($"Uploaded new image for step ID {stepId}");
            }

            await _dbContext.SaveChangesAsync();
        }

   
        public async Task DeleteTaskStep(int taskId, int stepId)
        {
            _logger.LogInformation($"Deleting step ID {stepId} for task ID {taskId}");

            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            if (taskStep == null)
            {
                _logger.LogWarning($"Step ID {stepId} not found for task ID {taskId}");
                throw new KeyNotFoundException($"Step ID {stepId} not found for task ID {taskId}");
            }
            
            if (!string.IsNullOrEmpty(taskStep.ImageUrl))
            {
                var blobFileName = taskStep.ImageUrl.Split('/').Last();
                await _blobStorageService.DeleteImageAsync(blobFileName);
                _logger.LogInformation($"Deleted image for step ID {stepId}");
            }

            _dbContext.TaskSteps.Remove(taskStep);
            await _dbContext.SaveChangesAsync();
        }
    }
}
