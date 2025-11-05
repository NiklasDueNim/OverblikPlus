using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Shift;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILoggerService _logger;

    public ShiftController(IShiftService shiftService, ILoggerService logger)
    {
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetShifts([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Today;
        var to = toDate ?? DateTime.Today.AddDays(7);

        _logger.LogInfo($"Getting shifts from {from.Date} to {to.Date}");
        var result = await _shiftService.GetShiftsForDateRangeAsync(from, to);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetShiftsForUser(string userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Today;
        var to = toDate ?? DateTime.Today.AddDays(7);

        _logger.LogInfo($"Getting shifts for user {userId} from {from.Date} to {to.Date}");
        var result = await _shiftService.GetShiftsForUserAsync(userId, from, to);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto createShiftDto)
    {
        if (createShiftDto == null)
        {
            return BadRequest("Shift data is required");
        }

        _logger.LogInfo($"Creating shift for user {createShiftDto.UserId}");
        var result = await _shiftService.CreateShiftAsync(createShiftDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetShifts), new { id = result.Data.Id }, result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteShift(Guid id)
    {
        _logger.LogInfo($"Deleting shift with id {id}");
        var result = await _shiftService.DeleteShiftAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return NoContent();
    }
}
