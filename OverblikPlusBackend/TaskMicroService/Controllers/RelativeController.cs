using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Mvc;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class RelativeController : ControllerBase
{
    private readonly IRelativeService _relativeService; //Husk DI i program

    public RelativeController(IRelativeService relativeService)
    {
        _relativeService = relativeService ?? throw new ArgumentNullException(nameof(relativeService));
    }
    
    
    [HttpGet("{userId}/tasks-for-day")]
    public async Task<ActionResult<IEnumerable<ReadTaskDto>>> GetTasksForDayForSpecificUser(string userId, [FromQuery] DateTime date)
    {
        try
        {
            var tasks = await _relativeService.GetTasksForDayForSpecificUser(userId, date);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}