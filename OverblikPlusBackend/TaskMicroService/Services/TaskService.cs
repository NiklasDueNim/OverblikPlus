using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskMicroService.Common;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services
{
    public class TaskService : ITaskService //TODO: Implementer transaction i create og update. Central logging og result
    {
        private readonly TaskDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;

        public TaskService(TaskDbContext dbContext, IMapper mapper, IBlobStorageService blobStorageService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetAllTasks()
        {
            var tasks = await _dbContext.Tasks.Include(t => t.Steps).ToListAsync();

            if (!tasks.Any())
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult("No tasks found.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        public async Task<Result<ReadTaskDto>> GetTaskById(int id)
        {
            var task = await _dbContext.Tasks.Include(t => t.Steps).FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return Result<ReadTaskDto>.ErrorResult($"Task with ID {id} not found.");

            var taskDto = _mapper.Map<ReadTaskDto>(task);
            return Result<ReadTaskDto>.SuccessResult(taskDto);
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksByUserId(string userId)
        {
            var tasks = await _dbContext.Tasks
                .Include(t => t.Steps)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (!tasks.Any())
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult($"No tasks found for user {userId}.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        public async Task<Result<int>> CreateTask(CreateTaskDto createTaskDto)
        {
            if (string.IsNullOrEmpty(createTaskDto.UserId))
                return Result<int>.ErrorResult("UserId is required for the task.");

            var taskEntity = _mapper.Map<TaskEntity>(createTaskDto);

            if (!string.IsNullOrEmpty(createTaskDto.ImageBase64))
            {
                var imageBytes = Convert.FromBase64String(createTaskDto.ImageBase64);
                using var stream = new MemoryStream(imageBytes);
                var blobFileName = $"{Guid.NewGuid()}.jpg";
                taskEntity.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
                Log.Logger.Information($"Image URL: {taskEntity.ImageUrl}");

            }

            taskEntity.NextOccurrence = createTaskDto.RecurrenceType != "None"
                ? createTaskDto.StartDate
                : DateTime.MinValue;

            _dbContext.Tasks.Add(taskEntity);
            await _dbContext.SaveChangesAsync();

            return Result<int>.SuccessResult(taskEntity.Id);
        }

        public async Task<Result> DeleteTask(int id)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            if (!string.IsNullOrEmpty(task.ImageUrl))
            {
                var blobFileName = task.ImageUrl.Substring(task.ImageUrl.LastIndexOf('/') + 1);
                await _blobStorageService.DeleteImageAsync(blobFileName);
            }

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            return Result.SuccessResult();
        }

        public async Task<Result> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var taskEntity = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (taskEntity == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            _mapper.Map(updateTaskDto, taskEntity);

            if (!string.IsNullOrEmpty(updateTaskDto.ImageUrl))
            {
                var imageBytes = Convert.FromBase64String(updateTaskDto.ImageUrl);
                using var stream = new MemoryStream(imageBytes);
                var blobFileName = $"{Guid.NewGuid()}.jpg";
                taskEntity.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
            }

            await _dbContext.SaveChangesAsync();
            return Result.SuccessResult();
        }

        public async Task<Result> MarkTaskAsCompleted(int taskId)
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null)
                return Result.ErrorResult($"Task with ID {taskId} not found.");

            task.IsCompleted = true;

            if (!string.IsNullOrEmpty(task.RecurrenceType) && task.RecurrenceType != "None")
            {
                task.NextOccurrence = CalculateNextOccurrence(task.NextOccurrence, task.RecurrenceType, task.RecurrenceInterval);
                task.IsCompleted = false;
            }

            await _dbContext.SaveChangesAsync();
            return Result.SuccessResult();
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date)
        {
            var tasks = await _dbContext.Tasks
                .Include(t => t.Steps)
                .Where(t => t.UserId == userId && t.NextOccurrence.Date == date.Date)
                .ToListAsync();

            if (!tasks.Any())
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult($"No tasks found for user {userId} on {date.ToShortDateString()}.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        private DateTime CalculateNextOccurrence(DateTime startDate, string recurrenceType, int interval)
        {
            return recurrenceType switch
            {
                "Daily" => startDate.AddDays(interval),
                "Weekly" => startDate.AddDays(7 * interval),
                "Monthly" => startDate.AddMonths(interval),
                _ => throw new ArgumentException("Invalid recurrence type")
            };
        }
    }
}
