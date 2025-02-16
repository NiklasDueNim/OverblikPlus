using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class RelativeController : ControllerBase
{
    private readonly IRelativeService _relativeService;
    private readonly ILoggerService _logger;

    public RelativeController(IRelativeService relativeService, ILoggerService logger)
    {
        _relativeService = relativeService ?? throw new ArgumentNullException(nameof(relativeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpGet("{userId}/tasks-for-day")]
    public async Task<ActionResult<IEnumerable<ReadCalendarEventDto>>> GetTasksForDayForSpecificUser(string userId, [FromQuery, BindRequired] DateTime date)
    {
        try
        {
            _logger.LogInfo($"Fetching tasks for user with id: {userId} for date: {date}");
            var tasks = await _relativeService.GetTasksForDayForSpecificUser(userId, date);
            _logger.LogInfo($"Found {tasks.Count()} tasks for user with id: {userId} for date: {date}");
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
    
    [HttpGet("{userId}/events-for-day")]
    public async Task<ActionResult<IEnumerable<ReadCalendarEventDto>>> GetEventsForDayForSpecificUser(string userId, [FromQuery, BindRequired] DateTime date)
    {
        try
        {
            _logger.LogInfo($"Fetching events for user with id: {userId} for date: {date}");
            var events = await _relativeService.GetEventsForDayForSpecificUser(userId, date);
            _logger.LogInfo($"Found {events.Count()} events for user with id: {userId} for date: {date}");
            return Ok(events);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}