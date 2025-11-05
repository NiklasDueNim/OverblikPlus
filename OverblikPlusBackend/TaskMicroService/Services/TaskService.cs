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
                await _dbContext.SaveChangesAsync();
            }

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
                taskEntity.NextOccurrence = CalculateNextOccurrence(createTaskDto.StartDate,
                    createTaskDto.RecurrenceType, createTaskDto.RecurrenceInterval,
                    createTaskDto.MonthlyType, createTaskDto.MonthlyDay, createTaskDto.SelectedWeekDays,
                    createTaskDto.EndType, createTaskDto.EndAfterCount, createTaskDto.EndDate);

                _logger.LogInfo("Saving task...");
                await SaveTaskAsync(taskEntity);
                
                // Sæt SeriesId til Id for den første opgave i serien (hvis det er en gentagende opgave)
                // Dette skal gøres efter SaveTaskAsync fordi vi skal have taskEntity.Id først
                if (!string.IsNullOrEmpty(taskEntity.RecurrenceType) && taskEntity.RecurrenceType != "None")
                {
                    // After SaveChangesAsync, EF Core should have set the Id automatically
                    // But we need to ensure it's tracked - it should be since we just added it
                    if (taskEntity.Id > 0)
                    {
                        taskEntity.SeriesId = taskEntity.Id;
                        await _dbContext.SaveChangesAsync();
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
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return Result.ErrorResult($"Task with ID {id} not found.");

            // Hvis det er en gentagende opgave, slet hele serien
            if (!string.IsNullOrEmpty(task.RecurrenceType) && task.RecurrenceType != "None")
            {
                var seriesId = task.SeriesId ?? task.Id;
                
                // Find alle opgaver i samme serie
                var tasksInSeries = await _dbContext.Tasks
                    .Where(t => (t.SeriesId ?? t.Id) == seriesId)
                    .ToListAsync();

                _logger.LogInfo($"Deleting {tasksInSeries.Count} tasks in series {seriesId}");

                // Slet billeder for alle opgaver i serien
                foreach (var taskInSeries in tasksInSeries)
                {
                    if (!string.IsNullOrEmpty(taskInSeries.ImageUrl))
                    {
                        await _imageService.DeleteImageAsync(taskInSeries.ImageUrl);
                    }
                    _dbContext.Tasks.Remove(taskInSeries);
                }
            }
            else
            {
                // Slet kun denne ene opgave
                if (!string.IsNullOrEmpty(task.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(task.ImageUrl);
                }
                _dbContext.Tasks.Remove(task);
            }

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

            var today = DateTime.UtcNow.Date;
            _logger.LogInfo($"[MarkTaskAsCompleted] Task {taskId} '{task.Name}' (NextOccurrence: {task.NextOccurrence?.Date:yyyy-MM-dd}, IsCompleted: {task.IsCompleted}) -> marking as completed");

            // Markér completed
            task.IsCompleted = true;

            // Ikke gentagende? Så er vi færdige.
            if (string.IsNullOrEmpty(task.RecurrenceType) || task.RecurrenceType == "None")
            {
                try
                {
                    await _dbContext.SaveChangesAsync();
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
            var selectedWeekDays = new Dictionary<string, bool>();
            if (!string.IsNullOrWhiteSpace(task.SelectedWeekDays))
            {
                try
                {
                    selectedWeekDays = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, bool>>(task.SelectedWeekDays)
                        ?? new Dictionary<string, bool>();
                }
                catch
                {
                    // Ignorer parsing error -> tomt
                    selectedWeekDays = new Dictionary<string, bool>();
                }
            }

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
                    await _dbContext.SaveChangesAsync();
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
            var sameDayDups = await _dbContext.Tasks
                .Where(t => 
                    t.Id != taskId && // Ikke den samme opgave
                    (t.SeriesId ?? t.Id) == seriesId && // Samme serie
                    t.NextOccurrence.HasValue &&
                    t.NextOccurrence.Value.Date == occurrenceDate) // Samme dato
                .ToListAsync();

            if (sameDayDups.Any())
            {
                _logger.LogInfo($"[MarkTaskAsCompleted] Removing {sameDayDups.Count} same-day duplicates for series {seriesId} on {occurrenceDate:yyyy-MM-dd}");
                _dbContext.Tasks.RemoveRange(sameDayDups);
            }
            
            // Slet alle gamle opgaver fra fortiden i samme serie
            var oldTasks = await _dbContext.Tasks
                .Where(t => 
                    t.Id != taskId && // Ikke den samme opgave
                    (t.SeriesId ?? t.Id) == seriesId && // Samme serie
                    t.NextOccurrence.HasValue &&
                    t.NextOccurrence.Value.Date < today) // Opgaver fra fortiden
                .ToListAsync();

            if (oldTasks.Any())
            {
                _logger.LogInfo($"[MarkTaskAsCompleted] Found {oldTasks.Count} old tasks from the past in series {seriesId}, deleting them.");
                foreach (var oldTask in oldTasks)
                {
                    _dbContext.Tasks.Remove(oldTask);
                }
            }

            // Beregn næste forekomst fra den aktuelle forekomst (ikke fra "i dag")
            var nextOccurrence = CalculateNextOccurrence(
                occurrenceDate,
                task.RecurrenceType,
                task.RecurrenceInterval,
                task.MonthlyType,
                task.MonthlyDay,
                selectedWeekDays,
                task.EndType,
                task.EndAfterCount,
                task.EndDate
            );

            // Sørg for STRIKT senere end occurrenceDate
            while (nextOccurrence.Date <= occurrenceDate)
            {
                nextOccurrence = CalculateNextOccurrence(
                    nextOccurrence.Date,
                    task.RecurrenceType,
                    task.RecurrenceInterval,
                    task.MonthlyType,
                    task.MonthlyDay,
                    selectedWeekDays,
                    task.EndType,
                    task.EndAfterCount,
                    task.EndDate
                );
            }

            // Undgå kollisioner i serien: hop frem til en unik dato
            while (await _dbContext.Tasks.AnyAsync(t =>
                (t.SeriesId ?? t.Id) == seriesId &&
                t.NextOccurrence.HasValue &&
                t.NextOccurrence.Value.Date == nextOccurrence.Date))
            {
                _logger.LogInfo($"[MarkTaskAsCompleted] Date {nextOccurrence.Date:yyyy-MM-dd} already exists in series {seriesId}, skipping forward");
                nextOccurrence = CalculateNextOccurrence(
                    nextOccurrence.Date,
                    task.RecurrenceType,
                    task.RecurrenceInterval,
                    task.MonthlyType,
                    task.MonthlyDay,
                    selectedWeekDays,
                    task.EndType,
                    task.EndAfterCount,
                    task.EndDate
                );
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

            _dbContext.Tasks.Add(newTask);

            try
            {
                await _dbContext.SaveChangesAsync();
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
            var task = await _dbContext.Tasks.FindAsync(taskId);
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
                var duplicateTasks = await _dbContext.Tasks
                    .Where(t => 
                        t.Id != taskId && // Ikke den samme opgave
                        (t.SeriesId ?? t.Id) == seriesId && // Samme serie
                        t.NextOccurrence.HasValue &&
                        t.NextOccurrence.Value.Date >= today && // Fremtidige eller i dag
                        t.IsCompleted == false)
                    .OrderBy(t => t.NextOccurrence) // Sorter efter NextOccurrence
                    .ToListAsync();

                if (duplicateTasks.Any())
                {
                    // Slet alle duplikater (der skulle kun være én, men for sikkerheds skyld sletter vi alle)
                    foreach (var duplicateTask in duplicateTasks)
                    {
                        _dbContext.Tasks.Remove(duplicateTask);
                        var dupNextOccurrence = duplicateTask.NextOccurrence?.Date.ToString("yyyy-MM-dd") ?? "null";
                        var taskNextOccurrence = task.NextOccurrence?.Date.ToString("yyyy-MM-dd") ?? "null";
                        _logger.LogInfo($"Deleted duplicate task {duplicateTask.Id} (NextOccurrence: {dupNextOccurrence}) when uncompleting task {taskId} (NextOccurrence: {taskNextOccurrence})");
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return Result.SuccessResult();
        }

        public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDay(string userId, DateTime date)
        {
            var tasks = await _dbContext.Tasks
                .Include(t => t.Steps)
                .Where(t => t.UserId == userId && t.NextOccurrence.HasValue && t.NextOccurrence.Value.Date == date.Date)
                .ToListAsync();

            if (!tasks.Any())
                return Result<IEnumerable<ReadTaskDto>>.ErrorResult(
                    $"No tasks found for user {userId} on {date.ToShortDateString()}.");

            var taskDtos = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(taskDtos);
        }

        private DateTime CalculateNextOccurrence(DateTime startDate, string recurrenceType, int interval,
            string monthlyType = "SameDay", int monthlyDay = 1, Dictionary<string, bool> selectedWeekDays = null,
            string endType = "Never", int endAfterCount = 1, DateTime? endDate = null)
        {
            if (recurrenceType == "None") return startDate;

            var currentDate = startDate;
            var occurrenceCount = 0;

            while (occurrenceCount < 100) // Safety limit
            {
                currentDate = recurrenceType switch
                {
                    "Daily" => currentDate.AddDays(interval),
                    "Weekly" => GetNextWeeklyOccurrence(currentDate, interval, selectedWeekDays),
                    "Monthly" => CalculateMonthlyOccurrence(currentDate, interval, monthlyType, monthlyDay),
                    "Yearly" => currentDate.AddYears(interval),
                    _ => throw new ArgumentException("Invalid recurrence type")
                };

                occurrenceCount++;

                // Check end conditions
                if (endType == "After" && occurrenceCount >= endAfterCount) break;
                if (endType == "Date" && endDate.HasValue && currentDate > endDate.Value) break;

                // For weekly, check if we have a valid day
                if (recurrenceType == "Weekly" && IsValidWeekDay(currentDate, selectedWeekDays))
                {
                    return currentDate;
                }
                else if (recurrenceType != "Weekly")
                {
                    return currentDate;
                }
            }

            return currentDate;
        }

        private DateTime CalculateMonthlyOccurrence(DateTime startDate, int interval, string monthlyType,
            int monthlyDay)
        {
            var nextDate = startDate.AddMonths(interval);

            return monthlyType switch
            {
                "SameDay" => nextDate, // Beholder samme dag (kan være problematisk)
                "FirstDay" => new DateTime(nextDate.Year, nextDate.Month, 1),
                "LastDay" => new DateTime(nextDate.Year, nextDate.Month, 1).AddMonths(1).AddDays(-1),
                "SpecificDay" => GetSpecificDayInMonth(nextDate, monthlyDay),
                _ => nextDate
            };
        }

        private DateTime GetSpecificDayInMonth(DateTime month, int day)
        {
            // Sikrer at vi ikke overskrider månedens antal dage 
            var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            var targetDay = Math.Min(day, daysInMonth);

            return new DateTime(month.Year, month.Month, targetDay);
        }

        private DateTime GetNextWeeklyOccurrence(DateTime currentDate, int interval,
            Dictionary<string, bool> selectedWeekDays)
        {
            var nextDate = currentDate.AddDays(7 * interval);

            // Find next valid weekday
            for (int i = 0; i < 7; i++)
            {
                var dayName = GetDanishDayName(nextDate.DayOfWeek);
                if (selectedWeekDays?.ContainsKey(dayName) == true && selectedWeekDays[dayName])
                {
                    return nextDate;
                }

                nextDate = nextDate.AddDays(1);
            }

            return nextDate;
        }

        private bool IsValidWeekDay(DateTime date, Dictionary<string, bool> selectedWeekDays)
        {
            if (selectedWeekDays == null || !selectedWeekDays.Any()) return true;

            var dayName = GetDanishDayName(date.DayOfWeek);
            return selectedWeekDays.ContainsKey(dayName) && selectedWeekDays[dayName];
        }

        private string GetDanishDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Mandag",
                DayOfWeek.Tuesday => "Tirsdag",
                DayOfWeek.Wednesday => "Onsdag",
                DayOfWeek.Thursday => "Torsdag",
                DayOfWeek.Friday => "Fredag",
                DayOfWeek.Saturday => "Lørdag",
                DayOfWeek.Sunday => "Søndag",
                _ => ""
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