using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
using TaskMicroService.Entities;
using TaskMicroService.Profiles;
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
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TaskDbContext(options);

        _mapperMock = new Mock<IMapper>();
        _blobStorageServiceMock = new Mock<IBlobStorageService>();

        _taskService = new TaskService(_dbContext, _mapperMock.Object, _blobStorageServiceMock.Object);
    }

    [Fact]
    public async Task GetAllTasks_ShouldReturnAllTasks()
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
                new ReadTaskDto { Id = 1, Name = "Test Task", Image = "http://example.com/image.jpg", RequiresQrCodeScan = true, RecurrenceType = "Daily"}
            });

        // Act
        var result = await _taskService.GetAllTasks();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Task", result.First().Name);
        Assert.Equal("http://example.com/image.jpg", result.First().Image);
        Assert.True(result.First().RequiresQrCodeScan);
    }

    [Fact]
    public async Task CreateTask_ShouldSaveTaskToDatabase()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Name = "New Task",
            Description = "This is a new task",
            ImageBase64 = Convert.ToBase64String(new byte[] { 0x20, 0x20 }),
            IsCompleted = false,
            RecurrenceType = "Daily",
            RecurrenceInterval = 1,
            StartDate = DateTime.UtcNow,
            RequiresQrCodeScan = true,
            UserId = "user123"
        };

        _blobStorageServiceMock
            .Setup(b => b.UploadImageAsync(It.IsAny<MemoryStream>(), It.IsAny<string>()))
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

        // Act
        var taskId = await _taskService.CreateTask(createTaskDto);

        // Assert
        var task = await _dbContext.Tasks.FindAsync(taskId);
        Assert.NotNull(task);
        Assert.Equal(createTaskDto.Name, task.Name);
        Assert.Equal(createTaskDto.Description, task.Description);
        Assert.Equal(createTaskDto.UserId, task.UserId);
        Assert.Equal(createTaskDto.RecurrenceType, task.RecurrenceType);
        Assert.Equal(createTaskDto.RecurrenceInterval, task.RecurrenceInterval);
        Assert.Equal(createTaskDto.StartDate, task.NextOccurrence);
        Assert.Equal("http://example.com/uploaded-image.jpg", task.ImageUrl);
        Assert.False(task.IsCompleted);
        Assert.True(task.RequiresQrCodeScan);
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
            .Returns(new ReadTaskDto { Id = 2, Name = "Specific Task", Image = "http://example.com/image.jpg", RecurrenceType = "Daily"});

        // Act
        var result = await _taskService.GetTaskById(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Specific Task", result.Name);
        Assert.Equal("http://example.com/image.jpg", result.Image);
        Assert.Equal("Daily", result.RecurrenceType);
    }
    
    [Fact]
    public async Task DeleteTask_ShouldRemoveTaskFromDatabaseAndDeleteBlob()
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

        _blobStorageServiceMock
            .Setup(b => b.DeleteImageAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _taskService.DeleteTask(3);

        // Assert
        var deletedTask = await _dbContext.Tasks.FindAsync(3);
        Assert.Null(deletedTask);
        _blobStorageServiceMock.Verify(b => b.DeleteImageAsync("image.jpg"), Times.Once);
    }
    
    
    [Fact]
    public async Task UpdateTask_ShouldUpdateTaskFields()
    {
        var now = DateTime.UtcNow;
        var existingTask = new TaskEntity
        {
            Id = 4,
            Name = "Old Task",
            Description = "Old Description",
            RecurrenceType = "Daily",
            RecurrenceInterval = 1,
            NextOccurrence = now,
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
            ImageUrl = Convert.ToBase64String(new byte[] { 0x20, 0x20 }),
            IsCompleted = existingTask.IsCompleted,
            UserId = existingTask.UserId,           
            RequiresQrCodeScan = existingTask.RequiresQrCodeScan
        };

        _blobStorageServiceMock
            .Setup(b => b.UploadImageAsync(It.IsAny<MemoryStream>(), It.IsAny<string>()))
            .ReturnsAsync("http://example.com/updated-image.jpg");

        _mapperMock.Setup(m => m.Map(updateTaskDto, existingTask))
            .Callback<UpdateTaskDto, TaskEntity>((src, dest) =>
            {
                dest.Name = src.Name;
                dest.Description = src.Description;
                dest.RecurrenceType = src.RecurrenceType;
                dest.RecurrenceInterval = src.RecurrenceInterval;
                dest.IsCompleted = src.IsCompleted;
                dest.UserId = src.UserId;
                dest.RequiresQrCodeScan = src.RequiresQrCodeScan;
            });

        // Act
        await _taskService.UpdateTask(4, updateTaskDto);

        // Assert
        var updatedTask = await _dbContext.Tasks.FindAsync(4);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Task", updatedTask.Name);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal("Weekly", updatedTask.RecurrenceType);
        Assert.Equal(2, updatedTask.RecurrenceInterval);
        Assert.Equal("http://example.com/updated-image.jpg", updatedTask.ImageUrl);
    }
    
    
    [Fact]
    public void AutoMapper_ShouldMapUpdateTaskDtoToTaskEntity()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();

        var updateTaskDto = new UpdateTaskDto { Name = "Updated Task" };
        var taskEntity = new TaskEntity { Name = "Old Task" };

        mapper.Map(updateTaskDto, taskEntity);

        Assert.Equal("Updated Task", taskEntity.Name); // Forvent succes her
    }



    
    [Fact]
    public async Task GetTaskById_ShouldReturnNullIfTaskNotFound()
    {
        // Act
        var result = await _taskService.GetTaskById(999);

        // Assert
        Assert.Null(result);
    }
    
    
    [Fact]
    public async Task MarkTaskAsCompleted_ShouldUpdateCompletionStatusForRecurringTask()
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
        await _taskService.MarkTaskAsCompleted(1);

        // Assert
        var completedTask = await _dbContext.Tasks.FindAsync(1);
        Assert.NotNull(completedTask);
        
        Assert.False(completedTask.IsCompleted);
        
        Assert.Equal(now.AddDays(1).Date, completedTask.NextOccurrence.Date);
    }

    
    [Fact]
    public async Task CreateTask_ShouldThrowExceptionIfUserIdIsMissing()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Name = "Task Without UserId",
            Description = "Description",
            UserId = null
        };

        _mapperMock
            .Setup(m => m.Map<TaskEntity>(createTaskDto))
            .Returns(new TaskEntity
            {
                Name = createTaskDto.Name,
                Description = createTaskDto.Description,
                UserId = null
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _taskService.CreateTask(createTaskDto));

        // Assert exception message
        Assert.Equal("UserId is not set for the task.", exception.Message);
    }
    
    
    [Fact]
    public async Task GetTasksByUserId_ShouldReturnTasksForSpecificUser()
    {
        // Arrange
        var taskForUser1 = new TaskEntity { Id = 6, Name = "User1 Task", UserId = "user1", RecurrenceType = "Daily"};
        var taskForUser2 = new TaskEntity { Id = 7, Name = "User2 Task", UserId = "user2", RecurrenceType = "Daily"};
        _dbContext.Tasks.AddRange(taskForUser1, taskForUser2);
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(m => m.Map<List<ReadTaskDto>>(It.IsAny<List<TaskEntity>>()))
            .Returns(new List<ReadTaskDto>
            {
                new ReadTaskDto { Id = 6, Name = "User1 Task", UserId = "user1" , RecurrenceType = "Daily"}
            });

        // Act
        var result = await _taskService.GetTasksByUserId("user1");

        // Assert
        Assert.Single(result);
        Assert.Equal("User1 Task", result.First().Name);
        Assert.Equal("user1", result.First().UserId);
    }
 
    
    [Fact]
    public async Task MarkTaskAsCompleted_ShouldSetNextOccurrenceForWeeklyRecurrence()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = 8,
            Name = "Recurring Task",
            IsCompleted = false,
            RecurrenceType = "Weekly",
            RecurrenceInterval = 1,
            NextOccurrence = DateTime.UtcNow
        };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        // Act
        await _taskService.MarkTaskAsCompleted(8);

        // Assert
        var updatedTask = await _dbContext.Tasks.FindAsync(8);
        Assert.NotNull(updatedTask);
        Assert.False(updatedTask.IsCompleted);
        Assert.Equal(DateTime.UtcNow.AddDays(7).Date, updatedTask.NextOccurrence.Date); 
    }
    
    
    [Fact]
    public async Task UpdateTask_ShouldThrowKeyNotFoundExceptionIfTaskNotFound()
    {
        // Arrange
        var updateTaskDto = new UpdateTaskDto
        {
            Name = "Updated Task",
            Description = "Updated Description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.UpdateTask(999, updateTaskDto));
    }
}