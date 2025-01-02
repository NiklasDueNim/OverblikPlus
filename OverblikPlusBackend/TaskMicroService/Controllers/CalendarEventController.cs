using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarEventController : ControllerBase
    {
        private readonly ICalendarEventService _calendarEventService;
        private readonly ILoggerService _logger;
        private readonly IValidator<CreateCalendarEventDto> _validator;

        public CalendarEventController(
            ICalendarEventService calendarEventService,
            ILoggerService logger,
            IValidator<CreateCalendarEventDto> validator)
        {
            _calendarEventService = calendarEventService ?? throw new ArgumentNullException(nameof(calendarEventService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var result = await _calendarEventService.GetEventByIdAsync(id);

            if (!result.Success)
            {
                _logger.LogWarning($"Failed to retrieve event with ID {id}: {result.Error}");
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetEventsForUser(string userId)
        {
            var result = await _calendarEventService.GetAllEventsAsync(userId);

            if (!result.Success)
            {
                _logger.LogWarning($"Failed to retrieve events for user {userId}: {result.Error}");
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateCalendarEventDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for creating calendar event.");
                return BadRequest(validationResult.Errors);
            }

            var result = await _calendarEventService.CreateEventAsync(dto);

            if (!result.Success)
            {
                _logger.LogError("Failed to create event:", new Exception(result.Error));
                return BadRequest(result.Error);
            }

            _logger.LogInfo($"Event created successfully with ID {result.Data.Id}");
            return CreatedAtAction(nameof(GetEvent), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateCalendarEventDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for updating calendar event.");
                return BadRequest(validationResult.Errors);
            }

            var result = await _calendarEventService.UpdateEventAsync(id, dto);

            if (!result.Success)
            {
                _logger.LogError($"Failed to update event with ID {id}:", new Exception(result.Error));
                return BadRequest(result.Error);
            }

            _logger.LogInfo($"Event with ID {id} updated successfully.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _calendarEventService.DeleteEventAsync(id);

            if (!result.Success)
            {
                _logger.LogError($"Failed to delete event with ID {id}", new Exception(result.Error));
                return BadRequest(result.Error);
            }

            _logger.LogInfo($"Event with ID {id} deleted successfully.");
            return NoContent();
        }
    }
}