using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Task;
using TaskMicroService.dtos.TaskStep;
using TaskMicroService.Entities;
using TaskMicroService.Services;

namespace TaskMicroService.Test.UnitTests;

public class RelativeServiceTests
{
    private readonly Mock<DbSet<TaskEntity>> _dbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TaskDbContext _dbContext;
    private readonly RelativeService _relativeService;

    public RelativeServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new TaskDbContext(options);
        _dbSetMock = new Mock<DbSet<TaskEntity>>();
        _mapperMock = new Mock<IMapper>();
        _relativeService = new RelativeService(_dbContext, _mapperMock.Object);
    }

    [Fact]
    public async Task GetTasksForDayForSpecificUser_ReturnsTasksForGivenDate()
    {
        // Arrange
        var userId = "test-user";
        var date = new DateTime(2025, 01, 25);
        var tasks = new List<TaskEntity>
        {
            new TaskEntity
            {
                UserId = userId,
                StartDate = new DateTime(2025, 01, 25, 10, 0, 0),
                Name = "Task 1",
                RecurrenceType = "Daily"
            },
            new TaskEntity
            {
                UserId = userId,
                StartDate = new DateTime(2025, 01, 25, 15, 0, 0),
                Name = "Task 2",
                RecurrenceType = "Weekly"
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();

        var readTaskDtos = tasks.Select(t => new ReadTaskDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Steps = t.Steps.Select(s => new ReadTaskStepDto
            {
            }).ToList(),
            Image = t.ImageUrl,
            RequiresQrCodeScan = t.RequiresQrCodeScan,
            UserId = t.UserId,
            isCompleted = t.IsCompleted,
            RecurrenceType = t.RecurrenceType,
            RecurrenceInterval = t.RecurrenceInterval,
            StartDate = t.StartDate,
            NextOccurrence = t.NextOccurrence
        }).ToList();

        _mapperMock.Setup(m => m.Map<IEnumerable<ReadTaskDto>>(It.IsAny<IEnumerable<TaskEntity>>()))
            .Returns(readTaskDtos);

        // Act
        var result = await _relativeService.GetTasksForDayForSpecificUser(userId, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}