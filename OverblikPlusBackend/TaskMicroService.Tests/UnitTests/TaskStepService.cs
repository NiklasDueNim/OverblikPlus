using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.TaskStep;
using TaskMicroService.Entities;
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;
using Xunit;

public class TaskStepServiceTests
{
    private readonly TaskStepService _taskStepService;
    private readonly TaskDbContext _dbContext;
    private readonly Mock<IBlobStorageService> _mockBlobStorageService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<TaskStepService>> _mockLogger;

    public TaskStepServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TaskDbContext(options);
        _mockBlobStorageService = new Mock<IBlobStorageService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<TaskStepService>>();

      
        _taskStepService = new TaskStepService(
            _dbContext,
            _mockMapper.Object,
            _mockBlobStorageService.Object,
            _mockLogger.Object
        );
    }


    [Fact]
    public async Task CreateTaskStep_ReturnsId_AfterCreation()
    {
        // Arrange
        var createStepDto = new CreateTaskStepDto
        {
            TaskId = 1,
            ImageBase64 = "base64String"
        };

        var taskStep = new TaskStep
        {
            Id = 1,
            TaskId = 1,
            ImageUrl = "image.jpg"
        };

        _mockMapper.Setup(m => m.Map<TaskStep>(createStepDto)).Returns(taskStep);
        _mockBlobStorageService.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("image.jpg");

        // Act
        var result = await _taskStepService.CreateTaskStep(createStepDto);

        // Assert
        Assert.Equal(1, result);
    }

 
    [Fact]
    public async Task GetStepsForTask_ReturnsCorrectSteps()
    {
        // Arrange
        var taskId = 1;
        var taskSteps = new List<TaskStep>
        {
            new TaskStep { Id = 1, TaskId = taskId, StepNumber = 1, ImageUrl = "step1.jpg" },
            new TaskStep { Id = 2, TaskId = taskId, StepNumber = 2, ImageUrl = "step2.jpg" }
        };

        await _dbContext.TaskSteps.AddRangeAsync(taskSteps);
        await _dbContext.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<List<ReadTaskStepDto>>(It.IsAny<List<TaskStep>>()))
            .Returns(new List<ReadTaskStepDto>
            {
                new ReadTaskStepDto { Id = 1, Image = "step1.jpg" },
                new ReadTaskStepDto { Id = 2, Image = "step2.jpg" }
            });

        // Act
        var result = await _taskStepService.GetStepsForTask(taskId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("step1.jpg", result[0].Image);
        Assert.Equal("step2.jpg", result[1].Image);
    }


    [Fact]
    public async Task GetTaskStep_ReturnsCorrectStep()
    {
        // Arrange
        var taskId = 1;
        var stepId = 1;
        var taskStep = new TaskStep { Id = stepId, TaskId = taskId, StepNumber = 1, ImageUrl = "step1.jpg" };

        await _dbContext.TaskSteps.AddAsync(taskStep);
        await _dbContext.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<ReadTaskStepDto>(It.IsAny<TaskStep>()))
            .Returns(new ReadTaskStepDto { Id = stepId, Image = "step1.jpg" });

        // Act
        var result = await _taskStepService.GetTaskStep(taskId, stepId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("step1.jpg", result.Image);
    }

    [Fact]
    public async Task GetTaskStep_ReturnsNull_WhenStepNotFound()
    {
        // Act
        var result = await _taskStepService.GetTaskStep(1, 99);

        // Assert
        Assert.Null(result);
    }

 
    [Fact]
    public async Task UpdateTaskStep_UpdatesStep()
    {
        // Arrange
        var taskId = 1;
        var stepId = 1;
        var taskStep = new TaskStep { Id = stepId, TaskId = taskId, ImageUrl = "old.jpg" };

        await _dbContext.TaskSteps.AddAsync(taskStep);
        await _dbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskStepDto 
        { 
            ImageBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("dummyImageContent")) 
        };

        _mockBlobStorageService.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("new.jpg");

        // Act
        await _taskStepService.UpdateTaskStep(taskId, stepId, updateDto);

        // Assert
        var updatedStep = await _dbContext.TaskSteps.FindAsync(stepId);
        Assert.NotNull(updatedStep);
        Assert.Equal("new.jpg", updatedStep.ImageUrl);
    }


    [Fact]
    public async Task UpdateTaskStep_ThrowsException_WhenStepNotFound()
    {
        // Arrange
        var updateDto = new UpdateTaskStepDto { ImageBase64 = "newImageBase64" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _taskStepService.UpdateTaskStep(1, 99, updateDto));
    }


    [Fact]
    public async Task DeleteTaskStep_DeletesStep()
    {
        // Arrange
        var taskId = 1;
        var stepId = 1;
        var taskStep = new TaskStep { Id = stepId, TaskId = taskId, ImageUrl = "step.jpg" };

        await _dbContext.TaskSteps.AddAsync(taskStep);
        await _dbContext.SaveChangesAsync();

        // Act
        await _taskStepService.DeleteTaskStep(taskId, stepId);

        // Assert
        var deletedStep = await _dbContext.TaskSteps.FindAsync(stepId);
        Assert.Null(deletedStep);
    }

    [Fact]
    public async Task DeleteTaskStep_ThrowsException_WhenStepNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _taskStepService.DeleteTaskStep(1, 99));
    }
}
