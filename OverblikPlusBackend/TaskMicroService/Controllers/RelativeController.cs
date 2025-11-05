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
    public async Task<ActionResult<IEnumerable<ReadTaskDto>>> GetTasksForDayForSpecificUser(string userId, [FromQuery, BindRequired] DateTime date)
    {
        _logger.LogInfo($"Fetching tasks for user with id: {userId} for date: {date}");
        var result = await _relativeService.GetTasksForDayForSpecificUser(userId, date);
        
        if (!result.Success)
            return BadRequest(result);

        _logger.LogInfo($"Found {result.Data?.Count() ?? 0} tasks for user with id: {userId} for date: {date}");
        return Ok(result);
    }
    
    [HttpGet("{userId}/events-for-day")]
    public async Task<ActionResult<IEnumerable<ReadCalendarEventDto>>> GetEventsForDayForSpecificUser(string userId, [FromQuery, BindRequired] DateTime date)
    {
        _logger.LogInfo($"Fetching events for user with id: {userId} for date: {date}");
        var result = await _relativeService.GetEventsForDayForSpecificUser(userId, date);
        
        if (!result.Success)
            return BadRequest(result);

        _logger.LogInfo($"Found {result.Data?.Count() ?? 0} events for user with id: {userId} for date: {date}");
        return Ok(result);
    }
    
    [HttpGet("{userId}/events")]
    public async Task<ActionResult<IEnumerable<ReadCalendarEventDto>>> GetEventsForIntervalForUser(
        string userId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _relativeService.GetEventsForIntervalForUser(userId, from, to);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}