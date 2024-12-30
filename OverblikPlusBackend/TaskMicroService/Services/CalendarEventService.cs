using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class CalendarEventService : ICalendarEventService
{
    private readonly TaskDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CalendarEventService(TaskDbContext dbContext, IMapper mapper, ILoggerService logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<ReadCalendarEventDto>>> GetAllEventsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<ReadCalendarEventDto>>.ErrorResult("UserId cannot be empty.");

            var events = await _dbContext.CalendarEvents
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var eventDtos = _mapper.Map<IEnumerable<ReadCalendarEventDto>>(events);
            return Result<IEnumerable<ReadCalendarEventDto>>.SuccessResult(eventDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving events for user {userId}.", ex);
            return Result<IEnumerable<ReadCalendarEventDto>>.ErrorResult("Error retrieving events.");
        }
    }

    public async Task<Result<ReadCalendarEventDto?>> GetEventByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Result<ReadCalendarEventDto?>.ErrorResult("Invalid ID.");

            var calendarEvent = await _dbContext.CalendarEvents.FindAsync(id);
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
            await _dbContext.CalendarEvents.AddAsync(calendarEvent);
            await _dbContext.SaveChangesAsync();

            var resultDto = _mapper.Map<ReadCalendarEventDto>(calendarEvent);
            return Result<ReadCalendarEventDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating event.", ex);
            return Result<ReadCalendarEventDto>.ErrorResult("Error creating event.");
        }
    }

    public async Task<Result<bool>> UpdateEventAsync(int id, CreateCalendarEventDto dto)
    {
        try
        {
            var existingEvent = await _dbContext.CalendarEvents.FindAsync(id);
            if (existingEvent == null)
                return Result<bool>.ErrorResult("Event not found.");

            _mapper.Map(dto, existingEvent);
            _dbContext.CalendarEvents.Update(existingEvent);
            await _dbContext.SaveChangesAsync();

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating event with ID {id}.", ex);
            return Result<bool>.ErrorResult("Error updating event.");
        }
    }

    public async Task<Result<bool>> DeleteEventAsync(int id)
    {
        try
        {
            var eventToDelete = await _dbContext.Set<CalendarEvent>().FindAsync(id);
            
            if (eventToDelete == null)
            {
                _logger.LogWarning($"Event with ID {id} not found.");
                return Result<bool>.ErrorResult("Event not found.");
            }

            _dbContext.CalendarEvents.Remove(eventToDelete);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return Result<bool>.SuccessResult(true);
            }
            else
            {
                return Result<bool>.ErrorResult("Failed to delete the event.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting event with ID {id}.", ex);
            return Result<bool>.ErrorResult("Error deleting event.");
        }
    }
}
