using AutoMapper;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class RelativeService : IRelativeService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICalendarEventRepository _calendarEventRepository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public RelativeService(
        ITaskRepository taskRepository,
        ICalendarEventRepository calendarEventRepository,
        IMapper mapper,
        ILoggerService logger)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _calendarEventRepository = calendarEventRepository ?? throw new ArgumentNullException(nameof(calendarEventRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<ReadTaskDto>>> GetTasksForDayForSpecificUser(string userId, DateTime date)
    {
        try
        {
            _logger.LogInfo($"Getting tasks for user {userId} on date {date.Date}");

            var tasks = await _taskRepository.GetByUserIdAndDateAsync(userId, date);

            if (!tasks.Any())
            {
                _logger.LogInfo($"No tasks found for user {userId} on date {date.Date}");
                return Result<IEnumerable<ReadTaskDto>>.SuccessResult(Enumerable.Empty<ReadTaskDto>());
            }

            var mappedTasks = _mapper.Map<List<ReadTaskDto>>(tasks);
            return Result<IEnumerable<ReadTaskDto>>.SuccessResult(mappedTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tasks for user {userId} on date {date.Date}: {ex.Message}", ex);
            return Result<IEnumerable<ReadTaskDto>>.ErrorResult($"Error getting tasks: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ReadCalendarEventDto>>> GetEventsForDayForSpecificUser(string userId, DateTime date)
    {
        try
        {
            _logger.LogInfo($"Getting calendar events for user {userId} on date {date.Date}");

            var events = await _calendarEventRepository.GetEventsByUserIdAndDateAsync(userId, date);

            if (!events.Any())
            {
                _logger.LogInfo($"No calendar events found for user {userId} on date {date.Date}");
                return Result<IEnumerable<ReadCalendarEventDto>>.SuccessResult(Enumerable.Empty<ReadCalendarEventDto>());
            }

            var mappedEvents = _mapper.Map<List<ReadCalendarEventDto>>(events);
            return Result<IEnumerable<ReadCalendarEventDto>>.SuccessResult(mappedEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting calendar events for user {userId} on date {date.Date}: {ex.Message}", ex);
            return Result<IEnumerable<ReadCalendarEventDto>>.ErrorResult($"Error getting calendar events: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ReadCalendarEventDto>>> GetEventsForIntervalForUser(string userId, DateTime from, DateTime to)
    {
        try
        {
            _logger.LogInfo($"Getting calendar events for user {userId} from {from.Date} to {to.Date}");

            var events = await _calendarEventRepository.GetEventsByUserIdAndDateRangeAsync(userId, from, to);

            if (!events.Any())
            {
                _logger.LogInfo($"No calendar events found for user {userId} from {from.Date} to {to.Date}");
                return Result<IEnumerable<ReadCalendarEventDto>>.SuccessResult(Enumerable.Empty<ReadCalendarEventDto>());
            }

            var mappedEvents = _mapper.Map<List<ReadCalendarEventDto>>(events);
            return Result<IEnumerable<ReadCalendarEventDto>>.SuccessResult(mappedEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting calendar events for user {userId} from {from.Date} to {to.Date}: {ex.Message}", ex);
            return Result<IEnumerable<ReadCalendarEventDto>>.ErrorResult($"Error getting calendar events: {ex.Message}");
        }
    }
}