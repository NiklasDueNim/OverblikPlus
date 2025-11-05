using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Common;
using TaskMicroService.dtos.Activity;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILoggerService _logger;

    public ActivityController(IActivityService activityService, ILoggerService logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllActivities()
    {
        _logger.LogInfo("Getting all activities");
        var result = await _activityService.GetAllActivitiesAsync();
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("date/{date:datetime}")]
    public async Task<IActionResult> GetActivitiesForDate(DateTime date)
    {
        _logger.LogInfo($"Getting activities for date {date:yyyy-MM-dd}");
        var result = await _activityService.GetActivitiesForDateAsync(date);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("daterange")]
    public async Task<IActionResult> GetActivitiesForDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        _logger.LogInfo($"Getting activities for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        var result = await _activityService.GetActivitiesForDateRangeAsync(startDate, endDate);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetActivityById(Guid id)
    {
        _logger.LogInfo($"Getting activity by ID: {id}");
        var result = await _activityService.GetActivityByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPost]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityDto createActivityDto)
    {
        _logger.LogInfo($"Creating new activity: {createActivityDto.Title}");
        var result = await _activityService.CreateActivityAsync(createActivityDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetActivityById), new { id = result.Data }, result);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] CreateActivityDto updateActivityDto)
    {
        _logger.LogInfo($"Updating activity: {id}");
        var result = await _activityService.UpdateActivityAsync(id, updateActivityDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        _logger.LogInfo($"Deleting activity: {id}");
        var result = await _activityService.DeleteActivityAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{activityId:guid}/join")]
    public async Task<IActionResult> JoinActivity(Guid activityId, [FromBody] JoinActivityRequest request)
    {
        _logger.LogInfo($"User {request.UserId} joining activity {activityId}");
        var result = await _activityService.JoinActivityAsync(activityId, request.UserId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{activityId:guid}/leave")]
    public async Task<IActionResult> LeaveActivity(Guid activityId, [FromBody] LeaveActivityRequest request)
    {
        _logger.LogInfo($"User {request.UserId} leaving activity {activityId}");
        var result = await _activityService.LeaveActivityAsync(activityId, request.UserId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{activityId:guid}/can-join/{userId:guid}")]
    public async Task<IActionResult> CanUserJoinActivity(Guid activityId, Guid userId)
    {
        _logger.LogInfo($"Checking if user {userId} can join activity {activityId}");
        var result = await _activityService.CanUserJoinActivityAsync(activityId, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

public class JoinActivityRequest
{
    public Guid UserId { get; set; }
}

public class LeaveActivityRequest
{
    public Guid UserId { get; set; }
}
