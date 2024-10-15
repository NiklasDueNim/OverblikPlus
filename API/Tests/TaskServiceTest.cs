using Moq;
using API.Services;
using API.Dto;
using DataAccess;
using DataAccess.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class TaskServiceTests
{
    private TaskService _taskService;
    private Mock<IMapper> _mockMapper;
    private AppDbContext _dbContext;

    public TaskServiceTests()
    {
        SetupDatabase();
    }

    // Setup database method to create the context and ensure a user is always available
    private void SetupDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
                      .UseInMemoryDatabase(databaseName: "TestDatabase")
                      .Options;
        _dbContext = new AppDbContext(options);
        _mockMapper = new Mock<IMapper>();
        _taskService = new TaskService(_dbContext, _mockMapper.Object);

        // Ensure a user is available in the database for tasks
        if (!_dbContext.Users.Any())
        {
            var user = new UserEntity
            {
                Id = 1,
                Username = "TestUser",
                CPRNumber = "123456-7890",
                MedicationDetails = "Some medication details",
                Role = "Resident"
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
    }

    [Fact]
    public void GetAllTasks_ReturnsTaskList()
    {
        // Reset database and setup
        _dbContext.Database.EnsureDeleted();
        SetupDatabase();

        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Name = "Task 1", Description = "Description 1", ImageUrl = "http://image1.com", User = _dbContext.Users.First() },
            new TaskEntity { Id = 2, Name = "Task 2", Description = "Description 2", ImageUrl = "http://image2.com", User = _dbContext.Users.First() }
        };
        _dbContext.Tasks.AddRange(tasks);
        _dbContext.SaveChanges();

        var mappedTasks = tasks.Select(t => new ReadTaskDto { Id = t.Id, Name = t.Name, Description = t.Description }).ToList();
        _mockMapper.Setup(m => m.Map<List<ReadTaskDto>>(It.IsAny<List<TaskEntity>>())).Returns(mappedTasks);

        // Act
        var result = _taskService.GetAllTasks();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetTaskById_ReturnsCorrectTask()
    {
        // Reset database and setup
        _dbContext.Database.EnsureDeleted();
        SetupDatabase();

        // Arrange
        var task = new TaskEntity { Id = 1, Name = "Task 1", Description = "Description 1", ImageUrl = "http://image1.com", User = _dbContext.Users.First() };
        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();

        var mappedTask = new ReadTaskDto { Id = task.Id, Name = task.Name, Description = task.Description };
        _mockMapper.Setup(m => m.Map<ReadTaskDto>(task)).Returns(mappedTask);

        // Act
        var result = _taskService.GetTaskById(1);

        // Assert
        Assert.NotNull(result); // Check if a task is found
        Assert.Equal(1, result.Id);
        Assert.Equal("Task 1", result.Name);
    }

    [Fact]
    public void CreateTask_AddsNewTask()
    {
        // Reset database and setup
        _dbContext.Database.EnsureDeleted();
        SetupDatabase();

        // Arrange
        var createTaskDto = new CreateTaskDto { Name = "New Task", Description = "New Description", ImageUrl = "http://newimage.com" };
        var taskEntity = new TaskEntity { Name = "New Task", Description = "New Description", ImageUrl = "http://newimage.com", User = _dbContext.Users.First() };

        _mockMapper.Setup(m => m.Map<TaskEntity>(createTaskDto)).Returns(taskEntity);

        // Act
        var result = _taskService.CreateTask(createTaskDto);

        // Assert
        Assert.Equal(1, _dbContext.Tasks.Count());
    }

    [Fact]
    public void DeleteTask_RemovesTask()
    {
        // Reset database and setup
        _dbContext.Database.EnsureDeleted();
        SetupDatabase();

        // Arrange
        var task = new TaskEntity { Id = 1, Name = "Task to Delete", Description = "Description", ImageUrl = "http://image.com", User = _dbContext.Users.First() };
        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();

        // Act
        _taskService.DeleteTask(1);

        // Assert
        Assert.Equal(0, _dbContext.Tasks.Count());
    }

    [Fact]
    public void UpdateTask_UpdatesExistingTask()
    {
        // Reset database and setup
        _dbContext.Database.EnsureDeleted(); 
        SetupDatabase();

        // Arrange
        var task = new TaskEntity { Id = 1, Name = "Old Name", Description = "Old Description", ImageUrl = "http://oldimage.com", User = _dbContext.Users.First() };
        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();

        var updateTaskDto = new UpdateTaskDto { Name = "New Name", Description = "New Description", ImageUrl = "http://newimage.com" };

        // Ensure mapper setup is correct
        _mockMapper.Setup(m => m.Map(updateTaskDto, task)).Callback(() =>
        {
            task.Name = updateTaskDto.Name;
            task.Description = updateTaskDto.Description;
            task.ImageUrl = updateTaskDto.ImageUrl;
        });

        // Act
        _taskService.UpdateTask(1, updateTaskDto);
        _dbContext.SaveChanges();

        // Assert
        var updatedTask = _dbContext.Tasks.First(t => t.Id == 1);
    
        // Log differences
        Console.WriteLine($"Expected Name: {updateTaskDto.Name}, Actual Name: {updatedTask.Name}");
        Console.WriteLine($"Expected Description: {updateTaskDto.Description}, Actual Description: {updatedTask.Description}");
        Console.WriteLine($"Expected ImageUrl: {updateTaskDto.ImageUrl}, Actual ImageUrl: {updatedTask.ImageUrl}");

        // Assert with expected and actual values
        Assert.Equal(updateTaskDto.Name, updatedTask.Name);
        Assert.Equal(updateTaskDto.Description, updatedTask.Description);
        Assert.Equal(updateTaskDto.ImageUrl, updatedTask.ImageUrl);
    }

}
