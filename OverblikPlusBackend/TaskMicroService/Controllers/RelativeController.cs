using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class RelativeController : ControllerBase
{
    private readonly IRelativeService _relativeService; //Husk DI i program
    private readonly ILoggerService _logger;

    public RelativeController(IRelativeService relativeService, ILoggerService logger)
    {
        _relativeService = relativeService ?? throw new ArgumentNullException(nameof(relativeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpGet("{userId}/tasks-for-day")]
    public async Task<ActionResult<IEnumerable<ReadTaskDto>>> GetTasksForDayForSpecificUser(string userId, [FromQuery, BindRequired] DateTime date)
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
}