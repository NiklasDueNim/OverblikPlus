using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.Controllers;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Services.Interfaces;
using Xunit;

namespace TaskMicroService.Test.UnitTests;

public class CalendarEventControllerTests
{
    private readonly Mock<ICalendarEventService> _mockCalendarEventService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IValidator<CreateCalendarEventDto>> _mockValidator;
    private readonly CalendarEventController _controller;

    public CalendarEventControllerTests()
    {
        _mockCalendarEventService = new Mock<ICalendarEventService>();
        _mockLogger = new Mock<ILoggerService>();
        _mockValidator = new Mock<IValidator<CreateCalendarEventDto>>();
        _controller = new CalendarEventController(
            _mockCalendarEventService.Object,
            _mockLogger.Object,
            _mockValidator.Object);
    }

    [Fact]
    public async Task GetEvent_ReturnsOkResult_WhenEventExists()
    {
        // Arrange
        var eventId = 1;
        var eventDto = new ReadCalendarEventDto { Title = "Test Event" };
        _mockCalendarEventService.Setup(s => s.GetEventByIdAsync(eventId))!
            .ReturnsAsync(Result<ReadCalendarEventDto>.SuccessResult(eventDto));

        // Act
        var result = await _controller.GetEvent(eventId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(eventDto, okResult.Value);
    }

    [Fact]
    public async Task GetEvent_ReturnsBadRequest_WhenEventDoesNotExist()
    {
        // Arrange
        var eventId = 1;
        _mockCalendarEventService.Setup(s => s.GetEventByIdAsync(eventId))!
            .ReturnsAsync(Result<ReadCalendarEventDto>.ErrorResult("Event not found"));

        // Act
        var result = await _controller.GetEvent(eventId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Event not found", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateEvent_ReturnsCreatedAtAction_WhenEventIsCreated()
    {
        // Arrange
        var eventDto = new CreateCalendarEventDto { Title = "Test Event" };
        var readEventDto = new ReadCalendarEventDto { Title = "Test Event" };
        _mockValidator.Setup(v => v.ValidateAsync(eventDto, default))
            .ReturnsAsync(new ValidationResult());
        _mockCalendarEventService.Setup(s => s.CreateEventAsync(eventDto))
            .ReturnsAsync(Result<ReadCalendarEventDto>.SuccessResult(readEventDto));

        // Act
        var result = await _controller.CreateEvent(eventDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(readEventDto, createdAtActionResult.Value);
    }

    [Fact]
    public async Task CreateEvent_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var eventDto = new CreateCalendarEventDto { Title = "Test Event" };
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required")
        };
        _mockValidator.Setup(v => v.ValidateAsync(eventDto, default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _controller.CreateEvent(eventDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(validationFailures, badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateEvent_ReturnsNoContent_WhenEventIsUpdated()
    {
        // Arrange
        var eventId = 1;
        var eventDto = new CreateCalendarEventDto { Title = "Updated Event" };
        _mockValidator.Setup(v => v.ValidateAsync(eventDto, default))
            .ReturnsAsync(new ValidationResult());
        _mockCalendarEventService.Setup(s => s.UpdateEventAsync(eventId, eventDto))
            .ReturnsAsync(Result<bool>.SuccessResult(true));

        // Act
        var result = await _controller.UpdateEvent(eventId, eventDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteEvent_ReturnsNoContent_WhenEventIsDeleted()
    {
        // Arrange
        var eventId = 1;
        _mockCalendarEventService.Setup(s => s.DeleteEventAsync(eventId))
            .ReturnsAsync(Result<bool>.SuccessResult(true));

        // Act
        var result = await _controller.DeleteEvent(eventId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}