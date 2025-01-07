using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Task;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly ILoggerService _logger;

        public TaskService(ITaskDbContext dbContext, IMapper mapper, IImageService imageService, ILoggerService logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetAllTasks()
        {
            _logger.LogInfo("Getting all tasks.");
            var tasks = await _dbContext.Tasks.Include(t => t.Steps).ToListAsync();

            if (!tasks.Any())
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult("No tasks found.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        public async Task<Result<ReadTaskDto>> GetTaskById(int id)
        {
            _logger.LogInfo($"Getting task with id = {id}");

            var task = await _dbContext.Tasks.Include(t => t.Steps).FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return Result<ReadTaskDto>.ErrorResult($"Task with ID {id} not found.");

            var taskDto = _mapper.Map<ReadTaskDto>(task);
            return Result<ReadTaskDto>.SuccessResult(taskDto);
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksByUserId(string userId)
        {
            _logger.LogInfo($"Getting all tasks from user {userId}");

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
            _logger.LogInfo($"Creating new task for user = {createTaskDto.UserId}");
            
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var taskEntity = _mapper.Map<TaskEntity>(createTaskDto);

                if (!string.IsNullOrEmpty(createTaskDto.ImageBase64))
                {
                    taskEntity.ImageUrl = await UploadImageAsync(createTaskDto.ImageBase64);
                }

                _logger.LogInfo("Calculating next occurrence...");
                taskEntity.NextOccurrence = CalculateNextOccurrence(createTaskDto.StartDate, createTaskDto.RecurrenceType, createTaskDto.RecurrenceInterval);

                _logger.LogInfo("Saving task...");
                await SaveTaskAsync(taskEntity);

                await transaction.CommitAsync();

                _logger.LogInfo($"Task created successfully with ID = {taskEntity.Id}");
                return Result<int>.SuccessResult(taskEntity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Error creating task", ex);
                return Result<int>.ErrorResult("An error occurred while creating the task.");
            }
        }

        public async Task<Result> DeleteTask(int id)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            if (!string.IsNullOrEmpty(task.ImageUrl))
            {
                await _imageService.DeleteImageAsync(task.ImageUrl);
            }

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            return Result.SuccessResult();
        }

        public async Task<Result> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            _logger.LogInfo($"Updating task with id = {id}");
            var taskEntity = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (taskEntity == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            _mapper.Map(updateTaskDto, taskEntity);

            if (!string.IsNullOrEmpty(updateTaskDto.ImageBase64))
            {
                taskEntity.ImageUrl = await UploadImageAsync(updateTaskDto.ImageBase64);
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
                task.NextOccurrence =
                    CalculateNextOccurrence(task.NextOccurrence, task.RecurrenceType, task.RecurrenceInterval);
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
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult(
                    $"No tasks found for user {userId} on {date.ToShortDateString()}.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        private DateTime CalculateNextOccurrence(DateTime startDate, string recurrenceType, int interval)
        {
            return recurrenceType switch
            {
                "None" => startDate,
                "Daily" => startDate.AddDays(interval),
                "Weekly" => startDate.AddDays(7 * interval),
                "Monthly" => startDate.AddMonths(interval),
                "Yearly" => startDate.AddYears(interval),
                _ => throw new ArgumentException("Invalid recurrence type")
            };
        }

        private async Task<string> UploadImageAsync(string imageBase64)
        {
            var imageUrl = await _imageService.UploadImageAsync(imageBase64);
            _logger.LogInfo($"Image URL: {imageUrl}");
            return imageUrl;
        }

        private async Task SaveTaskAsync(TaskEntity taskEntity)
        {
            _dbContext.Tasks.Add(taskEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}