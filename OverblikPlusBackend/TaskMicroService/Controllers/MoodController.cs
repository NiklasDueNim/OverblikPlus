using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Mood;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoodController : ControllerBase
{
    private readonly IMoodService _moodService;
    private readonly ILoggerService _logger;

    public MoodController(IMoodService moodService, ILoggerService logger)
    {
        _moodService = moodService ?? throw new ArgumentNullException(nameof(moodService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<IActionResult> CreateMood([FromBody] CreateMoodDto createMoodDto)
    {
        if (createMoodDto == null)
        {
            _logger.LogWarning("CreateMood called with null DTO");
            return BadRequest("Mood data is required");
        }

        _logger.LogInfo($"Creating mood for user {createMoodDto.UserId}");
        var result = await _moodService.CreateMood(createMoodDto);

        if (!result.Success)
        {
            _logger.LogWarning($"Failed to create mood: {result.Error}");
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetMoodsForUser(string userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Now.AddDays(-30);
        var to = toDate ?? DateTime.Now;

        _logger.LogInfo($"Getting moods for user {userId} from {from.Date} to {to.Date}");
        var result = await _moodService.GetMoodsForUserAsync(userId, from, to);

        if (!result.Success)
        {
            _logger.LogWarning($"Failed to get moods: {result.Error}");
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("bosted/{bostedId}")]
    [Authorize(Roles = "Admin, Staff")]
    public async Task<IActionResult> GetMoodsForBosted(int bostedId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        // Verify that the user's bostedId matches the requested bostedId
        var userBostedIdClaim = User.FindFirst("bostedId")?.Value;
        if (string.IsNullOrEmpty(userBostedIdClaim) || !int.TryParse(userBostedIdClaim, out var userBostedId) || userBostedId != bostedId)
        {
            _logger.LogWarning($"User attempted to access moods for bosted {bostedId} but belongs to bosted {userBostedIdClaim}");
            return Forbid();
        }

        var from = fromDate ?? DateTime.Now.AddDays(-30);
        var to = toDate ?? DateTime.Now;

        _logger.LogInfo($"Getting moods for bosted {bostedId} from {from.Date} to {to.Date}");
        var result = await _moodService.GetMoodsForBostedAsync(bostedId, from, to);

        if (!result.Success)
        {
            _logger.LogWarning($"Failed to get moods for bosted: {result.Error}");
            return BadRequest(result);
        }

        return Ok(result);
    }
}
