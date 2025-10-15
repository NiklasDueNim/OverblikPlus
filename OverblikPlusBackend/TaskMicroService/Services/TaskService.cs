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
                taskEntity.NextOccurrence = CalculateNextOccurrence(createTaskDto.StartDate, createTaskDto.RecurrenceType, createTaskDto.RecurrenceInterval,
                    createTaskDto.MonthlyType, createTaskDto.MonthlyDay, createTaskDto.SelectedWeekDays, 
                    createTaskDto.EndType, createTaskDto.EndAfterCount, createTaskDto.EndDate);

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

            // if (!string.IsNullOrEmpty(task.RecurrenceType) && task.RecurrenceType != "None")
            // {
            //     // Parse SelectedWeekDays from JSON string
            //     var selectedWeekDays = new Dictionary<string, bool>();
            //     if (!string.IsNullOrEmpty(task.SelectedWeekDays))
            //     {
            //         try
            //         {
            //             selectedWeekDays = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(task.SelectedWeekDays) ?? new Dictionary<string, bool>();
            //         }
            //         catch
            //         {
            //             selectedWeekDays = new Dictionary<string, bool>();
            //         }
            //     }
            //     
            //     task.NextOccurrence = CalculateNextOccurrence(task.NextOccurrence, task.RecurrenceType, task.RecurrenceInterval,
            //         task.MonthlyType, task.MonthlyDay, selectedWeekDays, 
            //         task.EndType, task.EndAfterCount, task.EndDate);
            //     task.IsCompleted = false;
            // }

            await _dbContext.SaveChangesAsync();
            return Result.SuccessResult();
        }

        public async Task<Result> MarkTaskAsUnCompleted(int taskId)
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return  Result.ErrorResult($"Task with ID {taskId} not found.");
            }
            task.IsCompleted = false;
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
        
        private DateTime CalculateMonthlyOccurrence(DateTime startDate, int interval, string monthlyType, int monthlyDay)
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
        
        private DateTime GetNextWeeklyOccurrence(DateTime currentDate, int interval, Dictionary<string, bool> selectedWeekDays)
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