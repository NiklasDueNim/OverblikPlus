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

            foreach (var stepDto in stepDtos)
            {
                var originalStep = steps.FirstOrDefault(s => s.Id == stepDto.Id);
                if (originalStep?.ImageUrl != null)
                {
                    stepDto.Image = originalStep.ImageUrl;
                }
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
            // Mapper DTO til TaskStep-entitet
            var taskStep = _mapper.Map<TaskStep>(createStepDto);
    
            // Tjekker om der er et billede i base64-format
            if (!string.IsNullOrEmpty(createStepDto.ImageBase64))
            {
                // Konverter base64-streng til byte-array
                var imageBytes = Convert.FromBase64String(createStepDto.ImageBase64);
        
                // Opretter en MemoryStream fra byte-arrayet
                using var stream = new MemoryStream(imageBytes);
        
                // Genererer et unikt navn til blob-filen
                var blobFileName = $"{Guid.NewGuid()}.jpg";
        
                // Uploader billedet til blob storage og får URL'en til filen
                taskStep.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
            }

            // Tilføjer det nye task step til databasen
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
                // Map de andre properties
                _mapper.Map(updateStepDto, taskStep);

                if (!string.IsNullOrEmpty(updateStepDto.ImageBase64))
                {
                    // Slet gammelt billede, hvis det eksisterer
                    if (!string.IsNullOrEmpty(taskStep.ImageUrl))
                    {
                        var oldBlobFileName = taskStep.ImageUrl.Substring(taskStep.ImageUrl.LastIndexOf('/') + 1);
                        await _blobStorageService.DeleteImageAsync(oldBlobFileName);
                    }

                    // Konverter base64 til stream og upload til blob storage
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
                // Slet billede fra blob storage, hvis det eksisterer
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
