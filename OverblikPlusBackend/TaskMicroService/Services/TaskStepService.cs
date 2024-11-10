using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
using TaskMicroService.Entities;
using System.Text;

namespace TaskMicroService.Services
{
    public class TaskStepService : ITaskStepService
    {
        private readonly TaskDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly BlobStorageService _blobStorageService;

        public TaskStepService(TaskDbContext dbContext, IMapper mapper, BlobStorageService blobStorageService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId)
        {
            var steps = await _dbContext.TaskSteps
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();
            
            var stepDtos = _mapper.Map<List<ReadTaskStepDto>>(steps);

            
            for (int i = 0; i < stepDtos.Count; i++)
            {
                var stepDto = stepDtos[i];
                var originalStep = steps.FirstOrDefault(s => s.Id == stepDto.Id);

                if (originalStep?.ImageUrl != null)
                {
                    stepDto.Image = originalStep.ImageUrl;
                }
                
                stepDto.NextStepId = (i < stepDtos.Count - 1) ? stepDtos[i + 1].Id : null;
            }

            return stepDtos;
        }


        public async Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepId)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            var stepDto = _mapper.Map<ReadTaskStepDto>(taskStep);
            if (taskStep?.ImageUrl != null)
            {
                stepDto.Image = taskStep.ImageUrl;
            }
            return stepDto;
        }

        public async Task<int> CreateTaskStep(CreateTaskStepDto createStepDto)
        {
            var taskStep = _mapper.Map<TaskStep>(createStepDto);
            
            if (!string.IsNullOrEmpty(createStepDto.ImageBase64))
            {
                var imageBytes = Convert.FromBase64String(createStepDto.ImageBase64);
                
                using var stream = new MemoryStream(imageBytes);
                
                var blobFileName = $"{Guid.NewGuid()}.jpg";
                
                taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
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

                if (!string.IsNullOrEmpty(updateStepDto.ImageBase64))
                {
                    if (!string.IsNullOrEmpty(taskStep.ImageUrl))
                    {
                        var oldBlobFileName = taskStep.ImageUrl.Substring(taskStep.ImageUrl.LastIndexOf('/') + 1);
                        await _blobStorageService.DeleteImageAsync(oldBlobFileName);
                    }
                    
                    var imageBytes = Convert.FromBase64String(updateStepDto.ImageBase64);
                    using var stream = new MemoryStream(imageBytes);

                    var blobFileName = $"{Guid.NewGuid()}.jpg";
                    taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                }

                await _dbContext.SaveChangesAsync();
            }
        }


        public async Task DeleteTaskStep(int taskId, int stepId)
        {
            var taskStep = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == stepId);

            if (taskStep != null)
            {
                if (!string.IsNullOrEmpty(taskStep.ImageUrl))
                {
                    var blobFileName = taskStep.ImageUrl.Split('/').Last();
                    await _blobStorageService.DeleteImageAsync(blobFileName);
                }

                _dbContext.TaskSteps.Remove(taskStep);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
