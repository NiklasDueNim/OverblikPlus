using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Helpers;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Task;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;
using TaskMicroService.Services.Recurrence;

namespace TaskMicroService.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly ILoggerService _logger;
        private readonly IRecurrenceCalculator _recurrenceCalculator;

        public TaskService(
            ITaskRepository taskRepository,
            ITaskDbContext dbContext,
            IMapper mapper,
            IImageService imageService,
            ILoggerService logger,
            IRecurrenceCalculator recurrenceCalculator)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recurrenceCalculator = recurrenceCalculator ?? throw new ArgumentNullException(nameof(recurrenceCalculator));
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetAllTasks()
        {
            _logger.LogInfo("Getting all tasks.");
            var tasks = await _taskRepository.GetAllAsync();

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        public async Task<Result<ReadTaskDto>> GetTaskById(int id)
        {
            _logger.LogInfo($"Getting task with id = {id}");

            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return Result<ReadTaskDto>.ErrorResult($"Task with ID {id} not found.");

            var taskDto = _mapper.Map<ReadTaskDto>(task);
            return Result<ReadTaskDto>.SuccessResult(taskDto);
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksByUserId(string userId)
        {
            _logger.LogInfo($"Getting all tasks from user {userId}");

            var tasks = await _taskRepository.GetByUserIdAsync(userId);

            // Fix missing SeriesId for existing recurring tasks
            var tasksToUpdate = tasks
                .Where(t => !string.IsNullOrEmpty(t.RecurrenceType) && 
                           t.RecurrenceType != "None" && 
                           !t.SeriesId.HasValue)
                .ToList();

            if (tasksToUpdate.Any())
            {
                _logger.LogInfo($"Fixing {tasksToUpdate.Count} tasks with missing SeriesId");
                foreach (var task in tasksToUpdate)
                {
                    task.SeriesId = task.Id;
                }
                await _taskRepository.SaveChangesAsync();
            }

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        public async Task<Result<int>> CreateTask(CreateTaskDto createTaskDto)
        {
            _logger.LogInfo($"Creating new task for user = {createTaskDto.UserId}");

            // Note: TaskRepository uses the same DbContext, so we can still use transaction from DbContext
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var taskEntity = _mapper.Map<TaskEntity>(createTaskDto);

                if (!string.IsNullOrEmpty(createTaskDto.ImageBase64))
                {
                    taskEntity.ImageUrl = await UploadImageAsync(createTaskDto.ImageBase64);
                }

                _logger.LogInfo("Calculating next occurrence...");
                var recurrenceOptions = new RecurrenceOptions
                {
                    StartDate = createTaskDto.StartDate,
                    RecurrenceType = createTaskDto.RecurrenceType,
                    RecurrenceInterval = createTaskDto.RecurrenceInterval,
                    MonthlyType = createTaskDto.MonthlyType,
                    MonthlyDay = createTaskDto.MonthlyDay,
                    SelectedWeekDays = createTaskDto.SelectedWeekDays,
                    EndType = createTaskDto.EndType,
                    EndAfterCount = createTaskDto.EndAfterCount,
                    EndDate = createTaskDto.EndDate
                };
                taskEntity.NextOccurrence = _recurrenceCalculator.CalculateNext(createTaskDto.StartDate, recurrenceOptions);

                _logger.LogInfo("Saving task...");
                await _taskRepository.AddAsync(taskEntity);
                await _taskRepository.SaveChangesAsync();
                
                // Sæt SeriesId til Id for den første opgave i serien (hvis det er en gentagende opgave)
                // Dette skal gøres efter SaveChangesAsync fordi vi skal have taskEntity.Id først
                if (!string.IsNullOrEmpty(taskEntity.RecurrenceType) && taskEntity.RecurrenceType != "None")
                {
                    // After SaveChangesAsync, EF Core should have set the Id automatically
                    // But we need to ensure it's tracked - it should be since we just added it
                    if (taskEntity.Id > 0)
                    {
                        taskEntity.SeriesId = taskEntity.Id;
                        await _taskRepository.SaveChangesAsync();
                        _logger.LogInfo($"Set SeriesId={taskEntity.SeriesId} for task {taskEntity.Id}");
                    }
                    else
                    {
                        _logger.LogWarning($"Task entity Id is still 0 after SaveTaskAsync - this should not happen");
                    }
                }

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
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            // Hvis det er en gentagende opgave OG har en SeriesId, slet hele serien
            if (!string.IsNullOrEmpty(task.RecurrenceType) && 
                task.RecurrenceType != "None" && 
                task.SeriesId.HasValue)
            {
                var seriesId = task.SeriesId.Value;
                
                // Find alle opgaver i samme serie - kun dem med eksakt samme SeriesId
                var tasksInSeries = await _taskRepository.GetBySeriesIdAsync(seriesId);

                _logger.LogInfo($"Deleting {tasksInSeries.Count} tasks in series {seriesId}");

                // Slet billeder for alle opgaver i serien
                foreach (var taskInSeries in tasksInSeries)
                {
                    if (!string.IsNullOrEmpty(taskInSeries.ImageUrl))
                    {
                        await _imageService.DeleteImageAsync(taskInSeries.ImageUrl);
                    }
                }
                await _taskRepository.DeleteRangeAsync(tasksInSeries);
            }
            else
            {
                // Slet kun denne ene opgave (enten ikke-gentagende, eller gentagende uden SeriesId)
                _logger.LogInfo($"Deleting single task {id} (RecurrenceType: {task.RecurrenceType}, SeriesId: {task.SeriesId})");
                if (!string.IsNullOrEmpty(task.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(task.ImageUrl);
                }
                await _taskRepository.DeleteAsync(task);
            }

            await _taskRepository.SaveChangesAsync();

            return Result.SuccessResult();
        }

        public async Task<Result> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            _logger.LogInfo($"Updating task with id = {id}");
            var taskEntity = await _taskRepository.GetByIdAsync(id);
            if (taskEntity == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            _mapper.Map(updateTaskDto, taskEntity);

            if (!string.IsNullOrEmpty(updateTaskDto.ImageBase64))
            {
                taskEntity.ImageUrl = await UploadImageAsync(updateTaskDto.ImageBase64);
            }

            await _taskRepository.UpdateAsync(taskEntity);
            await _taskRepository.SaveChangesAsync();
            return Result.SuccessResult();
        }

        public async Task<Result> MarkTaskAsCompleted(int taskId, DateTime occurrenceDate)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                return Result.ErrorResult($"Task with ID {taskId} not found.");

            var day = occurrenceDate.Date;
            var existing = await _taskRepository.GetCompletionAsync(taskId, day);
            if (existing == null)
            {
                await _taskRepository.AddCompletionAsync(new TaskCompletion
                {
                    TaskId = taskId,
                    UserId = task.UserId,
                    OccurrenceDate = day
                });
                await _taskRepository.SaveChangesAsync();
                _logger.LogInfo($"[MarkTaskAsCompleted] Task {taskId} completed for {day:yyyy-MM-dd}");
            }

            return Result.SuccessResult();
        }

        public async Task<Result> MarkTaskAsUnCompleted(int taskId, DateTime occurrenceDate)
        {
            var day = occurrenceDate.Date;
            var existing = await _taskRepository.GetCompletionAsync(taskId, day);
            if (existing != null)
            {
                await _taskRepository.RemoveCompletionAsync(existing);
                await _taskRepository.SaveChangesAsync();
                _logger.LogInfo($"[MarkTaskAsUnCompleted] Task {taskId} un-completed for {day:yyyy-MM-dd}");
            }

            return Result.SuccessResult();
        }

        public async Task<Result<IEnumerable<TaskCompletionDto>>> GetCompletions(string userId, DateTime from, DateTime to)
        {
            var completions = await _taskRepository.GetCompletionsForUserAsync(userId, from, to);
            var dtos = completions.Select(c => new TaskCompletionDto
            {
                TaskId = c.TaskId,
                OccurrenceDate = c.OccurrenceDate
            });
            return Result<IEnumerable<TaskCompletionDto>>.SuccessResult(dtos);
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date)
        {
            var tasks = await _taskRepository.GetByUserIdAndDateAsync(userId, date);

            if (!tasks.Any())
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult(
                    $"No tasks found for user {userId} on {date.ToShortDateString()}.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        private async Task<string> UploadImageAsync(string imageBase64)
        {
            var imageUrl = await _imageService.UploadImageAsync(imageBase64);
            _logger.LogInfo($"Image URL: {imageUrl}");
            return imageUrl;
        }
    }
}