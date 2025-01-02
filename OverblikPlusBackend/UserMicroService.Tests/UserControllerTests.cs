using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OverblikPlus.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserMicroService.Controllers;
using UserMicroService.dto;
using UserMicroService.Services.Interfaces;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IValidator<CreateUserDto>> _createUserValidatorMock;
    private readonly Mock<IValidator<UpdateUserDto>> _updateUserValidatorMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _createUserValidatorMock = new Mock<IValidator<CreateUserDto>>();
        _updateUserValidatorMock = new Mock<IValidator<UpdateUserDto>>();
        _loggerMock = new Mock<ILoggerService>();
        _controller = new UserController(
            _userServiceMock.Object,
            _createUserValidatorMock.Object,
            _updateUserValidatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonexistent";
        _userServiceMock.Setup(s => s.GetUserById(userId, "Admin")).ReturnsAsync((ReadUserDto)null);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value.GetType().GetProperty("Message").GetValue(notFoundResult.Value, null);
        Assert.Equal($"User with ID {userId} not found.", response);
    }

    [Fact]
    public async Task GetUserById_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var userId = "existing";
        var userDto = new ReadUserDto { Id = userId, Email = "test@example.com" };
        _userServiceMock.Setup(s => s.GetUserById(userId, "Admin")).ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(userDto, okResult.Value);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsNotFound_WhenNoUsersExist()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(Enumerable.Empty<ReadUserDto>());

        // Act
        var result = await _controller.GetAllUsersAsync();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value.GetType().GetProperty("Message").GetValue(notFoundResult.Value, null);
        Assert.Equal("No users found.", response);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsOk_WhenUsersExist()
    {
        // Arrange
        var users = new List<ReadUserDto> { new ReadUserDto { Id = "1", Email = "test1@example.com" } };
        _userServiceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsersAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(users, okResult.Value);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var createUserDto = new CreateUserDto();
        _createUserValidatorMock.Setup(v => v.ValidateAsync(createUserDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Email", "Email is required") }));

        // Act
        var result = await _controller.CreateUserAsync(createUserDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsCreated_WhenUserIsCreated()
    {
        // Arrange
        var createUserDto = new CreateUserDto { Email = "test@example.com", Password = "password" };
        _createUserValidatorMock.Setup(v => v.ValidateAsync(createUserDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _userServiceMock.Setup(s => s.CreateUserAsync(createUserDto)).ReturnsAsync("newUserId");

        // Act
        var result = await _controller.CreateUserAsync(createUserDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("newUserId", createdResult.RouteValues["id"]);
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var updateUserDto = new UpdateUserDto();
        _updateUserValidatorMock.Setup(v => v.ValidateAsync(updateUserDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Email", "Email is required") }));

        // Act
        var result = await _controller.UpdateUserAsync("existing", updateUserDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var updateUserDto = new UpdateUserDto { Email = "test@example.com" };
        _updateUserValidatorMock.Setup(v => v.ValidateAsync(updateUserDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _userServiceMock.Setup(s => s.GetUserById("nonexistent", "Admin")).ReturnsAsync((ReadUserDto)null);

        // Act
        var result = await _controller.UpdateUserAsync("nonexistent", updateUserDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value.GetType().GetProperty("Message").GetValue(notFoundResult.Value, null);
        Assert.Equal("User with ID nonexistent not found.", response);
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsNoContent_WhenUserIsUpdated()
    {
        // Arrange
        var updateUserDto = new UpdateUserDto { Email = "test@example.com" };
        _updateUserValidatorMock.Setup(v => v.ValidateAsync(updateUserDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _userServiceMock.Setup(s => s.GetUserById("existing", "Admin")).ReturnsAsync(new ReadUserDto { Id = "existing" });

        // Act
        var result = await _controller.UpdateUserAsync("existing", updateUserDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonexistentUserId";
        _userServiceMock.Setup(s => s.GetUserById(userId, "Admin")).ReturnsAsync((ReadUserDto)null);

        // Act
        var result = await _controller.DeleteUserAsync(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value.GetType().GetProperty("Message").GetValue(notFoundResult.Value, null);
        Assert.Equal($"User with ID {userId} not found.", response);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsNoContent_WhenUserIsDeleted()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetUserById("existing", "Admin")).ReturnsAsync(new ReadUserDto { Id = "existing" });

        // Act
        var result = await _controller.DeleteUserAsync("existing");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}