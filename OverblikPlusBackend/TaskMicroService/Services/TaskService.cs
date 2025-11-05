using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Task;
using TaskMicroService.Entities;
using TaskMicroService.Helpers;
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

        public async Task<Result> MarkTaskAsCompleted(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                return Result.ErrorResult($"Task with ID {taskId} not found.");

            var today = DateTime.UtcNow.Date;
            _logger.LogInfo($"[MarkTaskAsCompleted] Task {taskId} '{task.Name}' (NextOccurrence: {task.NextOccurrence?.Date:yyyy-MM-dd}, IsCompleted: {task.IsCompleted}) -> marking as completed");

            // Markér completed
            task.IsCompleted = true;

            // Ikke gentagende? Så er vi færdige.
            if (string.IsNullOrEmpty(task.RecurrenceType) || task.RecurrenceType == "None")
            {
                try
                {
                    await _taskRepository.UpdateAsync(task);
                    await _taskRepository.SaveChangesAsync();
                    _logger.LogInfo($"[MarkTaskAsCompleted] Task {taskId} is not recurring, marked as completed");
                    return Result.SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    var innerMessage = ex.InnerException?.Message ?? ex.Message;
                    _logger.LogError($"[MarkTaskAsCompleted] Save failed for task {taskId}: {innerMessage}", ex);
                    return Result.ErrorResult($"Could not save changes. {innerMessage}");
                }
            }

            // Parse SelectedWeekDays from JSON string
            var selectedWeekDays = JsonHelper.Deserialize<Dictionary<string, bool>>(task.SelectedWeekDays ?? string.Empty)
                ?? new Dictionary<string, bool>();

            // Bestem SeriesId for denne opgave (brug eksisterende eller sæt til Id)
            var seriesId = task.SeriesId ?? task.Id;
            var occurrenceDate = (task.NextOccurrence ?? today).Date;

            // VIGTIGT: Opret KUN ny forekomst hvis den aktuelle forekomst er forfalden (<= i dag)
            var shouldCreateNext = occurrenceDate <= today;

            if (!shouldCreateNext)
            {
                // Fremtidig forekomst blev "forhåndsafsluttet" – ingen ny opgave skal oprettes.
                _logger.LogInfo($"[MarkTaskAsCompleted] Task {taskId} has future occurrence date {occurrenceDate:yyyy-MM-dd}, not creating next occurrence");
                try
                {
                    await _taskRepository.UpdateAsync(task);
                    await _taskRepository.SaveChangesAsync();
                    return Result.SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    var innerMessage = ex.InnerException?.Message ?? ex.Message;
                    _logger.LogError($"[MarkTaskAsCompleted] Save failed for task {taskId}: {innerMessage}", ex);
                    return Result.ErrorResult($"Could not save changes. {innerMessage}");
                }
            }

            // Slet same-day duplicates (midlertidig cleanup - indexet forhindrer nye, men der kan være gammel støj)
            var sameDayDups = await _taskRepository.GetSameDayDuplicatesAsync(seriesId, occurrenceDate, taskId);

            if (sameDayDups.Any())
            {
                _logger.LogInfo($"[MarkTaskAsCompleted] Removing {sameDayDups.Count} same-day duplicates for series {seriesId} on {occurrenceDate:yyyy-MM-dd}");
                await _taskRepository.DeleteRangeAsync(sameDayDups);
            }
            
            // Slet alle gamle opgaver fra fortiden i samme serie
            var oldTasks = await _taskRepository.GetOldTasksInSeriesAsync(seriesId, today, taskId);

            if (oldTasks.Any())
            {
                _logger.LogInfo($"[MarkTaskAsCompleted] Found {oldTasks.Count} old tasks from the past in series {seriesId}, deleting them.");
                await _taskRepository.DeleteRangeAsync(oldTasks);
            }

            // Beregn næste forekomst fra den aktuelle forekomst (ikke fra "i dag")
            var recurrenceOptions = new RecurrenceOptions
            {
                StartDate = occurrenceDate,
                RecurrenceType = task.RecurrenceType,
                RecurrenceInterval = task.RecurrenceInterval,
                MonthlyType = task.MonthlyType,
                MonthlyDay = task.MonthlyDay,
                SelectedWeekDays = selectedWeekDays,
                EndType = task.EndType,
                EndAfterCount = task.EndAfterCount,
                EndDate = task.EndDate
            };
            var nextOccurrence = _recurrenceCalculator.CalculateNext(occurrenceDate, recurrenceOptions);

            // Sørg for STRIKT senere end occurrenceDate
            while (nextOccurrence.Date <= occurrenceDate)
            {
                recurrenceOptions.StartDate = nextOccurrence.Date;
                nextOccurrence = _recurrenceCalculator.CalculateNext(nextOccurrence.Date, recurrenceOptions);
            }

            // Undgå kollisioner i serien: hop frem til en unik dato
            while (await _taskRepository.ExistsWithDateAsync(seriesId, nextOccurrence.Date))
            {
                _logger.LogInfo($"[MarkTaskAsCompleted] Date {nextOccurrence.Date:yyyy-MM-dd} already exists in series {seriesId}, skipping forward");
                recurrenceOptions.StartDate = nextOccurrence.Date;
                nextOccurrence = _recurrenceCalculator.CalculateNext(nextOccurrence.Date, recurrenceOptions);
            }

            _logger.LogInfo($"[MarkTaskAsCompleted] Calculated next occurrence for task {taskId}: {nextOccurrence.Date:yyyy-MM-dd} (occurrenceDate: {occurrenceDate:yyyy-MM-dd}, today: {today:yyyy-MM-dd})");

            // Opret næste forekomst
            _logger.LogInfo($"[MarkTaskAsCompleted] Creating new task for next occurrence {nextOccurrence.Date:yyyy-MM-dd} in series {seriesId}");
            var newTask = new TaskEntity
            {
                Name = task.Name,
                Description = task.Description,
                ImageUrl = task.ImageUrl,
                RecurrenceType = task.RecurrenceType,
                RecurrenceInterval = task.RecurrenceInterval,
                StartDate = task.StartDate,
                NextOccurrence = nextOccurrence,
                UserId = task.UserId,
                RequiresQrCodeScan = task.RequiresQrCodeScan,
                IsCompleted = false,
                MonthlyType = task.MonthlyType,
                MonthlyDay = task.MonthlyDay,
                SelectedWeekDays = task.SelectedWeekDays,
                EndType = task.EndType,
                EndAfterCount = task.EndAfterCount,
                EndDate = task.EndDate,
                SeriesId = seriesId
            };

            await _taskRepository.AddAsync(newTask);

            try
            {
                await _taskRepository.SaveChangesAsync();
                _logger.LogInfo($"[MarkTaskAsCompleted] Task {taskId} marked as completed successfully");
                return Result.SuccessResult();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError($"[MarkTaskAsCompleted] Save failed for task {taskId}: {innerMessage}", ex);
                return Result.ErrorResult($"Could not save changes. {innerMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[MarkTaskAsCompleted] Unexpected error for task {taskId}: {ex.Message}", ex);
                return Result.ErrorResult($"An unexpected error occurred: {ex.Message}");
            }
        }


        public async Task<Result> MarkTaskAsUnCompleted(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return Result.ErrorResult($"Task with ID {taskId} not found.");
            }

            task.IsCompleted = false;

            // Hvis det er en gentagende opgave, find og slet den nye opgave der blev oprettet
            // når markeringen blev sat (hvis den findes)
            if (!string.IsNullOrEmpty(task.RecurrenceType) && task.RecurrenceType != "None")
            {
                var today = DateTime.Today;
                var seriesId = task.SeriesId ?? task.Id;
                
                // Find alle opgaver i samme serie der er fremtidige ELLER i dag
                // Disse kan være opgaver der blev oprettet ved fejl
                var tasksInSeries = await _taskRepository.GetBySeriesIdAsync(seriesId);
                var duplicateTasks = tasksInSeries
                    .Where(t => 
                        t.Id != taskId && // Ikke den samme opgave
                        t.NextOccurrence.HasValue &&
                        t.NextOccurrence.Value.Date >= today && // Fremtidige eller i dag
                        t.IsCompleted == false)
                    .OrderBy(t => t.NextOccurrence) // Sorter efter NextOccurrence
                    .ToList();

                if (duplicateTasks.Any())
                {
                    // Slet alle duplikater (der skulle kun være én, men for sikkerheds skyld sletter vi alle)
                    foreach (var duplicateTask in duplicateTasks)
                    {
                        var dupNextOccurrence = duplicateTask.NextOccurrence?.Date.ToString("yyyy-MM-dd") ?? "null";
                        var taskNextOccurrence = task.NextOccurrence?.Date.ToString("yyyy-MM-dd") ?? "null";
                        _logger.LogInfo($"Deleted duplicate task {duplicateTask.Id} (NextOccurrence: {dupNextOccurrence}) when uncompleting task {taskId} (NextOccurrence: {taskNextOccurrence})");
                    }
                    await _taskRepository.DeleteRangeAsync(duplicateTasks);
                }
            }

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();
            return Result.SuccessResult();
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