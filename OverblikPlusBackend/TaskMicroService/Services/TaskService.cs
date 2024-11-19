using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly BlobStorageService _blobStorageService;

        public TaskService(TaskDbContext dbContext, IMapper mapper, BlobStorageService blobStorageService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        public async Task<IEnumerable<ReadTaskDto>> GetAllTasks()
        {
            var tasks = await _dbContext.Tasks
                .Include(t => t.Steps)
                .ToListAsync();

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);

            foreach (var taskDto in taskDtos)
            {
                var taskEntity = tasks.FirstOrDefault(t => t.Id == taskDto.Id);
                if (!string.IsNullOrEmpty(taskEntity?.ImageUrl))
                {
                    taskDto.Image = taskEntity.ImageUrl;
                }
                taskDto.RequiresQrCodeScan = taskEntity.RequiresQrCodeScan;
            }

            return taskDtos;
        }

        public async Task<ReadTaskDto> GetTaskById(int id)
        {
            var task = await _dbContext.Tasks
                .Include(t => t.Steps)
                .FirstOrDefaultAsync(t => t.Id == id);

            var taskDto = _mapper.Map<ReadTaskDto>(task);

            if (!string.IsNullOrEmpty(task?.ImageUrl))
            {
                taskDto.Image = task.ImageUrl;
            }
            taskDto.RequiresQrCodeScan = task?.RequiresQrCodeScan ?? false;

            return taskDto;
        }
        

        public async Task<IEnumerable<ReadTaskDto>> GetTasksByUserId(string userId)
        {
            var tasks = await _dbContext.Tasks
                .Include(t => t.Steps)
                .Where(t => t.UserId == userId)
                .ToListAsync();
            
            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            
            foreach (var taskDto in taskDtos)
            {
                var taskEntity = tasks.FirstOrDefault(t => t.Id == taskDto.Id);
                if (!string.IsNullOrEmpty(taskEntity?.ImageUrl))
                {
                    taskDto.Image = taskEntity.ImageUrl;
                }
                taskDto.RequiresQrCodeScan = taskEntity.RequiresQrCodeScan;
            }

            return taskDtos;
        }


        public async Task<int> CreateTask(CreateTaskDto createTaskDto)
        {
            var task = _mapper.Map<TaskEntity>(createTaskDto);
            
            if (!string.IsNullOrEmpty(createTaskDto.ImageBase64))
            {
                var imageBytes = Convert.FromBase64String(createTaskDto.ImageBase64);
                using var stream = new MemoryStream(imageBytes);
                
                var blobFileName = $"{Guid.NewGuid()}.jpg"; 
                task.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
            }

            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            return task.Id;
        }

        public async Task DeleteTask(int id)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task != null)
            {
                if (!string.IsNullOrEmpty(task.ImageUrl))
                {
                    var blobFileName = task.ImageUrl.Substring(task.ImageUrl.LastIndexOf('/') + 1);
                    await _blobStorageService.DeleteImageAsync(blobFileName);
                }

                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var taskEntity = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (taskEntity != null)
            {
                _mapper.Map(updateTaskDto, taskEntity);
                
                if (!string.IsNullOrEmpty(updateTaskDto.ImageUrl))
                {
                    var imageBytes = Convert.FromBase64String(updateTaskDto.ImageUrl);
                    using var stream = new MemoryStream(imageBytes);

                    var blobFileName = $"{Guid.NewGuid()}.jpg";
                    taskEntity.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
