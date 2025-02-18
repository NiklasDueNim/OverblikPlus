using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class RelativeService : IRelativeService
{
    private readonly TaskDbContext _dbContext;
    private readonly IMapper _mapper;

    public RelativeService(TaskDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReadTaskDto>> GetTasksForDayForSpecificUser(string userId, DateTime date)
    {
        var tasks = await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.StartDate.Date == date.Date)
            .ToListAsync();

        if (tasks == null || !tasks.Any())
        {
            return Enumerable.Empty<ReadTaskDto>();
        }

        var mappedTasks = _mapper.Map<IEnumerable<ReadTaskDto>>(tasks);

        if (mappedTasks == null || !mappedTasks.Any())
        {
            throw new InvalidOperationException("Mapping from TaskEntity to ReadTaskDto failed or resulted in an empty list.");
        }

        return mappedTasks;
    }

    public async Task<IEnumerable<ReadCalendarEventDto>> GetEventsForDayForSpecificUser(string userId, DateTime date)
    {
        var events = await _dbContext.CalendarEvents
            .Where(e => e.UserId == userId && e.StartDateTime.Date == date.Date)
            .ToListAsync();

        if (events == null || !events.Any())
        {
            return Enumerable.Empty<ReadCalendarEventDto>();
        }

        var mappedEvents = _mapper.Map<IEnumerable<ReadCalendarEventDto>>(events);

        if (mappedEvents == null || !mappedEvents.Any())
        {
            throw new InvalidOperationException("Mapping from CalendarEvent to ReadCalendarEventDto failed or resulted in an empty list.");
        }

        return mappedEvents;
    }

    public async Task<IEnumerable<ReadCalendarEventDto>> GetEventsForIntervalForUser(string userId, DateTime from, DateTime to)
    {
        var events = await _dbContext.CalendarEvents
            .Where(e => e.UserId == userId && e.StartDateTime >= from && e.StartDateTime <= to)
            .ToListAsync();

        if (events == null || !events.Any())
        {
            return Enumerable.Empty<ReadCalendarEventDto>();
        }

        var mappedEvents = _mapper.Map<IEnumerable<ReadCalendarEventDto>>(events);

        if (mappedEvents == null || !mappedEvents.Any())
        {
            throw new InvalidOperationException("Mapping from CalendarEvent to ReadCalendarEventDto failed or resulted in an empty list.");
        }

        return mappedEvents;
    }
}