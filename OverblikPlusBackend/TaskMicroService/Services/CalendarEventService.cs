using AutoMapper;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class CalendarEventService : ICalendarEventService
{
    private readonly ICalendarEventRepository _calendarEventRepository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CalendarEventService(ICalendarEventRepository calendarEventRepository, IMapper mapper, ILoggerService logger)
    {
        _calendarEventRepository = calendarEventRepository ?? throw new ArgumentNullException(nameof(calendarEventRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<ReadCalendarEventDto>>> GetAllEventsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<ReadCalendarEventDto>>.ErrorResult("UserId cannot be empty.");

            var events = await _calendarEventRepository.GetEventsByUserIdAsync(userId);
            var eventDtos = _mapper.Map<IEnumerable<ReadCalendarEventDto>>(events);
            return Result<IEnumerable<ReadCalendarEventDto>>.SuccessResult(eventDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving events for user {userId}.", ex);
            return Result<IEnumerable<ReadCalendarEventDto>>.ErrorResult("Error retrieving events.");
        }
    }

    public async Task<Result<ReadCalendarEventDto?>> GetEventByIdAsync(Guid id)
    {
        try
        {
            var calendarEvent = await _calendarEventRepository.GetByIdAsync(id);
            if (calendarEvent == null)
                return Result<ReadCalendarEventDto?>.ErrorResult("Event not found.");

            var eventDto = _mapper.Map<ReadCalendarEventDto>(calendarEvent);
            return Result<ReadCalendarEventDto?>.SuccessResult(eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving event with ID {id}.", ex);
            return Result<ReadCalendarEventDto?>.ErrorResult("Error retrieving event.");
        }
    }

    public async Task<Result<ReadCalendarEventDto>> CreateEventAsync(CreateCalendarEventDto dto)
    {
        try
        {
            var calendarEvent = _mapper.Map<CalendarEvent>(dto);
            await _calendarEventRepository.AddAsync(calendarEvent);
            await _calendarEventRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<ReadCalendarEventDto>(calendarEvent);
            return Result<ReadCalendarEventDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating event.", ex);
            return Result<ReadCalendarEventDto>.ErrorResult("Error creating event.");
        }
    }

    public async Task<Result<bool>> UpdateEventAsync(Guid id, CreateCalendarEventDto dto)
    {
        try
        {
            var existingEvent = await _calendarEventRepository.GetByIdAsync(id);
            if (existingEvent == null)
                return Result<bool>.ErrorResult("Event not found.");

            _mapper.Map(dto, existingEvent);
            await _calendarEventRepository.UpdateAsync(existingEvent);
            await _calendarEventRepository.SaveChangesAsync();

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating event with ID {id}.", ex);
            return Result<bool>.ErrorResult("Error updating event.");
        }
    }

    public async Task<Result<bool>> DeleteEventAsync(Guid id)
    {
        try
        {
            var eventToDelete = await _calendarEventRepository.GetByIdAsync(id);
            
            if (eventToDelete == null)
            {
                _logger.LogWarning($"Event with ID {id} not found.");
                return Result<bool>.ErrorResult("Event not found.");
            }

            await _calendarEventRepository.DeleteAsync(eventToDelete);
            await _calendarEventRepository.SaveChangesAsync();

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting event with ID {id}.", ex);
            return Result<bool>.ErrorResult("Error deleting event.");
        }
    }
}
