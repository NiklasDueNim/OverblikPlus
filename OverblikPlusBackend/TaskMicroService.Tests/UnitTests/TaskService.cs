using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Logging;
using Serilog;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Task;
using TaskMicroService.Entities;
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Test.UnitTests;

public class TaskServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
    private readonly TaskDbContext _dbContext;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new TaskDbContext(options);

        _mapperMock = new Mock<IMapper>();
        var imageServiceMock = new Mock<IImageService>();

        _taskService = new TaskService(_dbContext, _mapperMock.Object, imageServiceMock.Object, new Mock<ILoggerService>().Object);
    }

    [Fact]
    public async Task GetAllTasks_ShouldReturnSuccessResultWithAllTasks()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = 1,
            Name = "Test Task",
            Steps = new List<TaskStep>(),
            ImageUrl = "http://example.com/image.jpg",
            RequiresQrCodeScan = true,
            RecurrenceType = "Daily"
        };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(m => m.Map<List<ReadTaskDto>>(It.IsAny<List<TaskEntity>>()))
            .Returns(new List<ReadTaskDto>
            {
                new ReadTaskDto { Id = 1, Name = "Test Task", Image = "http://example.com/image.jpg", RequiresQrCodeScan = true, RecurrenceType = "Daily" }
            });

        // Act
        var result = await _taskService.GetAllTasks();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Test Task", result.Data.First().Name);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnCorrectTask()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = 2,
            Name = "Specific Task",
            UserId = "user123",
            ImageUrl = "http://example.com/image.jpg",
            RequiresQrCodeScan = false,
            RecurrenceType = "Daily"
        };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(m => m.Map<ReadTaskDto>(It.IsAny<TaskEntity>()))
            .Returns(new ReadTaskDto { Id = 2, Name = "Specific Task", Image = "http://example.com/image.jpg", RecurrenceType = "Daily" });

        // Act
        var result = await _taskService.GetTaskById(2);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Id);
        Assert.Equal("Specific Task", result.Data.Name);
        Assert.Equal("http://example.com/image.jpg", result.Data.Image);
        Assert.Equal("Daily", result.Data.RecurrenceType);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnErrorIfTaskNotFound()
    {
        // Act
        int id = 999;
        var result = await _taskService.GetTaskById(id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal($"Task with ID {id} not found.", result.Error);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnSuccessResultWithTaskId()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Name = "New Task",
            Description = "This is a new task",
            ImageBase64 = Convert.ToBase64String(new byte[] { 0x20, 0x20 }),
            RecurrenceType = "Daily",
            RecurrenceInterval = 1,
            StartDate = DateTime.UtcNow,
            RequiresQrCodeScan = true,
            UserId = "user123"
        };

        var imageServiceMock = new Mock<IImageService>();
        imageServiceMock
            .Setup(i => i.UploadImageAsync(It.IsAny<string>()))
            .ReturnsAsync("http://example.com/uploaded-image.jpg");

        _mapperMock
            .Setup(m => m.Map<TaskEntity>(createTaskDto))
            .Returns(new TaskEntity
            {
                Name = createTaskDto.Name,
                Description = createTaskDto.Description,
                RecurrenceType = createTaskDto.RecurrenceType,
                RecurrenceInterval = createTaskDto.RecurrenceInterval,
                NextOccurrence = createTaskDto.StartDate,
                UserId = createTaskDto.UserId,
                RequiresQrCodeScan = createTaskDto.RequiresQrCodeScan
            });

        var taskService = new TaskService(_dbContext, _mapperMock.Object, imageServiceMock.Object, new Mock<ILoggerService>().Object);

        // Act
        var result = await taskService.CreateTask(createTaskDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, result.Error);
        Assert.NotNull(result.Data);
        Assert.True(result.Data > 0);
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnSuccessResult()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = 3,
            Name = "Task to Delete",
            ImageUrl = "http://example.com/image.jpg",
            RecurrenceType = "Daily"
        };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var imageServiceMock = new Mock<IImageService>();
        imageServiceMock
            .Setup(i => i.DeleteImageAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var taskService = new TaskService(_dbContext, _mapperMock.Object, imageServiceMock.Object, new Mock<ILoggerService>().Object);

        // Act
        var result = await taskService.DeleteTask(3);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        var deletedTask = await _dbContext.Tasks.FindAsync(3);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnErrorIfTaskNotFound()
    {
        // Act
        int id = 999;
        var result = await _taskService.DeleteTask(id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal($"Task with ID {id} not found.", result.Error);
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnSuccessIfTaskUpdated()
    {
        // Arrange
        var existingTask = new TaskEntity
        {
            Id = 4,
            Name = "Old Task",
            Description = "Old Description",
            RecurrenceType = "Daily",
            RecurrenceInterval = 1,
            UserId = "user123"
        };

    _dbContext.Tasks.Add(existingTask);
    await _dbContext.SaveChangesAsync();

    var updateTaskDto = new UpdateTaskDto
    {
        Name = "Updated Task",
        Description = "Updated Description",
        RecurrenceType = "Weekly",
        RecurrenceInterval = 2,
        UserId = "user123"
    };

    _mapperMock
        .Setup(m => m.Map(updateTaskDto, existingTask))
        .Callback<UpdateTaskDto, TaskEntity>((src, dest) =>
        {
            dest.Name = src.Name;
            dest.Description = src.Description;
            dest.RecurrenceType = src.RecurrenceType;
            dest.RecurrenceInterval = src.RecurrenceInterval;
        });

    // Act
    var result = await _taskService.UpdateTask(existingTask.Id, updateTaskDto);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);

    var updatedTask = await _dbContext.Tasks.FindAsync(existingTask.Id);
    Assert.NotNull(updatedTask);
    Assert.Equal("Updated Task", updatedTask.Name);
    Assert.Equal("Updated Description", updatedTask.Description);
    Assert.Equal("Weekly", updatedTask.RecurrenceType); 
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnErrorIfTaskNotFound()
    {
        // Arrange
        int id = 999;
        var updateTaskDto = new UpdateTaskDto
        {
            Name = "Non-existent Task"
        };

        // Act
        var result = await _taskService.UpdateTask(id, updateTaskDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal($"Task with ID {id} not found.", result.Error);
    }

    [Fact]
    public async Task MarkTaskAsCompleted_ShouldReturnSuccessForValidTask()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var task = new TaskEntity
        {
            Id = 1,
            Name = "Test Task",
            IsCompleted = false,
            RecurrenceType = "Daily",
            RecurrenceInterval = 1,
            NextOccurrence = now
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _taskService.MarkTaskAsCompleted(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        var updatedTask = await _dbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.False(updatedTask.IsCompleted);
        Assert.Equal(now.AddDays(1).Date, updatedTask.NextOccurrence.Date);
    }

    [Fact]
    public async Task MarkTaskAsCompleted_ShouldReturnErrorIfTaskNotFound()
    {
        // Arrange
        int id = 999;
        
        // Act
        var result = await _taskService.MarkTaskAsCompleted(id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal($"Task with ID {id} not found.", result.Error);
    }

    [Fact]
    public async Task GetTasksByUserId_ShouldReturnTasksForUser()
    {
        // Arrange
        var taskForUser = new TaskEntity
        {
            Id = 6,
            Name = "User Task",
            UserId = "user1",
            RecurrenceType = "Daily"
        };

        _dbContext.Tasks.Add(taskForUser);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(m => m.Map<List<ReadTaskDto>>(It.IsAny<List<TaskEntity>>()))
            .Returns(new List<ReadTaskDto>
            {
                new ReadTaskDto { Id = 6, Name = "User Task", UserId = "user1" }
            });

        // Act
        var result = await _taskService.GetTasksByUserId("user1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("User Task", result.Data.First().Name);
        Assert.Equal("user1", result.Data.First().UserId);
    }

    [Fact]
    public async Task GetTasksForDay_ShouldReturnTasksForGivenDate()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var task = new TaskEntity
        {
            Id = 7,
            Name = "Daily Task",
            UserId = "user1",
            NextOccurrence = date,
            RecurrenceType = "Daily"
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(m => m.Map<List<ReadTaskDto>>(It.IsAny<List<TaskEntity>>()))
            .Returns(new List<ReadTaskDto>
            {
                new ReadTaskDto { Id = 7, Name = "Daily Task", UserId = "user1" }
            });

        // Act
        var result = await _taskService.GetTasksForDay("user1", date);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("Daily Task", result.Data.First().Name);
        Assert.Equal("user1", result.Data.First().UserId);
    }

    [Fact]
    public async Task GetTasksForDay_ShouldReturnErrorIfNoTasksFound()
    {
        // Arrange
        var userId = "user1";
        var date = DateTime.UtcNow.AddDays(1);
        
        // Act
        var result = await _taskService.GetTasksForDay("user1", DateTime.UtcNow.AddDays(1));

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal($"No tasks found for user {userId} on {date.ToShortDateString()}.", result.Error);
    }
}