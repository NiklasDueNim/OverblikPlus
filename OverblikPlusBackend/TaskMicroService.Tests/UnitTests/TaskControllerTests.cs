using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.Controllers;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Test.UnitTests;

public class TaskControllerTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IValidator<CreateTaskDto>> _mockCreateTaskValidator;
    private readonly Mock<IValidator<UpdateTaskDto>> _mockUpdateTaskValidator;
    private readonly TaskController _controller;

    public TaskControllerTests()
    {
        _mockTaskService = new Mock<ITaskService>();
        _mockLogger = new Mock<ILoggerService>();
        _mockCreateTaskValidator = new Mock<IValidator<CreateTaskDto>>();
        _mockUpdateTaskValidator = new Mock<IValidator<UpdateTaskDto>>();

        _controller = new TaskController(
            _mockTaskService.Object,
            _mockLogger.Object,
            _mockCreateTaskValidator.Object,
            _mockUpdateTaskValidator.Object
        );

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "User")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskNotExists()
    {
        // Arrange
        _mockTaskService.Setup(x => x.GetTaskById(It.IsAny<int>()))
            .ReturnsAsync(Result<ReadTaskDto>.ErrorResult("Task not found"));

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _mockTaskService.Setup(x => x.GetAllTasks())
            .ReturnsAsync(Result<IEnumerable<ReadTaskDto>>.ErrorResult("Service error"));

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto { Name = "Test Task" };
        _mockCreateTaskValidator.Setup(x => x.ValidateAsync(createTaskDto, default))
            .ReturnsAsync(new ValidationResult());

        _mockTaskService.Setup(x => x.CreateTask(createTaskDto))
            .ReturnsAsync(Result<int>.SuccessResult(1));

        // Act
        var result = await _controller.CreateTask(createTaskDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var resultValue = Assert.IsType<Result<int>>(createdResult.Value);
        Assert.Equal(1, resultValue.Data);
    }

    [Fact]
    public async Task UpdateTask_ReturnsOk_WhenUpdateIsValid()
    {
        // Arrange
        var updateTaskDto = new UpdateTaskDto { Name = "Updated Task" };
        _mockUpdateTaskValidator.Setup(x => x.ValidateAsync(updateTaskDto, default))
            .ReturnsAsync(new ValidationResult());

        _mockTaskService.Setup(x => x.UpdateTask(1, updateTaskDto))
            .ReturnsAsync(Result.SuccessResult());

        // Act
        var result = await _controller.UpdateTask(1, updateTaskDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
    
    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskNotExists()
    {
        // Arrange
        _mockTaskService.Setup(x => x.DeleteTask(1))
            .ReturnsAsync(Result.ErrorResult("Task not found"));

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}