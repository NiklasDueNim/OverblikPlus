using AutoMapper;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.dtos.TaskStep;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly ITaskStepRepository _taskStepRepository;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILoggerService _logger;

        public TaskStepService(
            ITaskStepRepository taskStepRepository, 
            IMapper mapper, 
            IBlobStorageService blobStorageService,
            ILoggerService logger)
        {
            _taskStepRepository = taskStepRepository ?? throw new ArgumentNullException(nameof(taskStepRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
        }
        
        public async Task<Result<List<ReadTaskStepDto>>> GetStepsForTask(int taskId)
        {
            try
            {
                _logger.LogInfo($"Fetching steps for task ID {taskId}");

                var steps = await _taskStepRepository.GetStepsForTaskAsync(taskId);
                
                if (!steps.Any())
                {
                    _logger.LogWarning($"No steps found for task ID {taskId}");
                    return Result<List<ReadTaskStepDto>>.SuccessResult(new List<ReadTaskStepDto>());
                }

                var stepDtos = _mapper.Map<List<ReadTaskStepDto>>(steps);
                for (int i = 0; i < stepDtos.Count; i++)
                {
                    var stepDto = stepDtos[i];
                    var originalStep = steps[i];

                    stepDto.Image = originalStep.ImageUrl;
                    stepDto.NextStepId = (i < stepDtos.Count - 1) ? stepDtos[i + 1].Id : null;
                }

                return Result<List<ReadTaskStepDto>>.SuccessResult(stepDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting steps for task {taskId}: {ex.Message}", ex);
                return Result<List<ReadTaskStepDto>>.ErrorResult($"Error getting steps: {ex.Message}");
            }
        }

        public async Task<Result<ReadTaskStepDto>> GetTaskStep(int taskId, int stepId)
        {
            try
            {
                _logger.LogInfo($"Fetching step ID {stepId} for task ID {taskId}");

                var taskStep = await _taskStepRepository.GetTaskStepAsync(taskId, stepId);

                if (taskStep == null)
                {
                    _logger.LogWarning($"Step ID {stepId} not found for task ID {taskId}");
                    return Result<ReadTaskStepDto>.ErrorResult($"Step ID {stepId} not found for task ID {taskId}");
                }

                var stepDto = _mapper.Map<ReadTaskStepDto>(taskStep);
                stepDto.Image = taskStep.ImageUrl;
                return Result<ReadTaskStepDto>.SuccessResult(stepDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting step {stepId} for task {taskId}: {ex.Message}", ex);
                return Result<ReadTaskStepDto>.ErrorResult($"Error getting step: {ex.Message}");
            }
        }
        
        public async Task<Result<int>> CreateTaskStep(CreateTaskStepDto createStepDto)
        {
            try
            {
                _logger.LogInfo($"Creating a new step for task ID {createStepDto.TaskId}");

                var taskStep = _mapper.Map<TaskStep>(createStepDto);
                
                if (!string.IsNullOrEmpty(createStepDto.ImageBase64))
                {
                    var imageBytes = Convert.FromBase64String(createStepDto.ImageBase64);
                    using var stream = new MemoryStream(imageBytes);
                    var blobFileName = $"{Guid.NewGuid()}.jpg";

                    taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                    _logger.LogInfo($"Uploaded image for step ID {taskStep.Id}");
                }
                
                await _taskStepRepository.AddAsync(taskStep);
                await _taskStepRepository.SaveChangesAsync();

                _logger.LogInfo($"Task step created successfully with ID {taskStep.Id}");
                return Result<int>.SuccessResult(taskStep.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating task step: {ex.Message}", ex);
                return Result<int>.ErrorResult($"Error creating task step: {ex.Message}");
            }
        }

        public async Task<Result> UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updateStepDto)
        {
            try
            {
                _logger.LogInfo($"Updating step ID {stepId} for task ID {taskId}");

                var taskStep = await _taskStepRepository.GetTaskStepAsync(taskId, stepId);

                if (taskStep == null)
                {
                    _logger.LogWarning($"Step ID {stepId} not found for task ID {taskId}");
                    return Result.ErrorResult($"Step ID {stepId} not found for task ID {taskId}");
                }

                _mapper.Map(updateStepDto, taskStep);

                if (!string.IsNullOrEmpty(updateStepDto.ImageBase64))
                {
                    if (!string.IsNullOrEmpty(taskStep.ImageUrl))
                    {
                        var oldBlobFileName = taskStep.ImageUrl.Split('/').Last();
                        await _blobStorageService.DeleteImageAsync(oldBlobFileName);
                        _logger.LogInfo($"Deleted old image for step ID {stepId}");
                    }
                    
                    var imageBytes = Convert.FromBase64String(updateStepDto.ImageBase64);
                    using var stream = new MemoryStream(imageBytes);
                    var blobFileName = $"{Guid.NewGuid()}.jpg";
                    taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                    _logger.LogInfo($"Uploaded new image for step ID {stepId}");
                }

                await _taskStepRepository.UpdateAsync(taskStep);
                await _taskStepRepository.SaveChangesAsync();

                _logger.LogInfo($"Task step updated successfully");
                return Result.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating task step: {ex.Message}", ex);
                return Result.ErrorResult($"Error updating task step: {ex.Message}");
            }
        }

        public async Task<Result> DeleteTaskStep(int taskId, int stepId)
        {
            try
            {
                _logger.LogInfo($"Deleting step ID {stepId} for task ID {taskId}");

                var taskStep = await _taskStepRepository.GetTaskStepAsync(taskId, stepId);

                if (taskStep == null)
                {
                    _logger.LogWarning($"Step ID {stepId} not found for task ID {taskId}");
                    return Result.ErrorResult($"Step ID {stepId} not found for task ID {taskId}");
                }
                
                if (!string.IsNullOrEmpty(taskStep.ImageUrl))
                {
                    var blobFileName = taskStep.ImageUrl.Split('/').Last();
                    await _blobStorageService.DeleteImageAsync(blobFileName);
                    _logger.LogInfo($"Deleted image for step ID {stepId}");
                }

                await _taskStepRepository.DeleteAsync(taskStep);
                await _taskStepRepository.SaveChangesAsync();

                _logger.LogInfo($"Task step deleted successfully");
                return Result.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting task step: {ex.Message}", ex);
                return Result.ErrorResult($"Error deleting task step: {ex.Message}");
            }
        }
    }
}
