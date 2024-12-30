using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Entities;
using TaskMicroService.Profiles;
using TaskMicroService.Services;

namespace TaskMicroService.Tests.Services
{
    public class CalendarEventServiceTests
    {
        private readonly Mock<TaskDbContext> _mockDbContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly CalendarEventService _service;

        public CalendarEventServiceTests()
        {
            _mockDbContext = new Mock<TaskDbContext>(new DbContextOptions<TaskDbContext>());
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();

            _service = new CalendarEventService(_mockDbContext.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllEventsAsync_ShouldReturnError_WhenUserIdIsEmpty()
        {
            // Act
            var result = await _service.GetAllEventsAsync("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("UserId cannot be empty.", result.Error);
        }

        [Fact]
        public async Task GetAllEventsAsync_ShouldReturnEvents_WhenUserIdIsValid()
        {
            // Arrange
            var userId = "test-user";
            var events = new List<CalendarEvent> { new CalendarEvent { Id = 1, UserId = userId } };
            var dtos = new List<ReadCalendarEventDto> { new ReadCalendarEventDto { Id = 1, UserId = userId } };

            _mockDbContext.Setup(db => db.CalendarEvents).ReturnsDbSet(events);
            _mockMapper.Setup(m => m.Map<IEnumerable<ReadCalendarEventDto>>(events)).Returns(dtos);

            // Act
            var result = await _service.GetAllEventsAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(dtos, result.Data);
        }

        [Fact]
        public async Task GetEventByIdAsync_ShouldReturnError_WhenIdIsInvalid()
        {
            // Act
            var result = await _service.GetEventByIdAsync(0);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid ID.", result.Error);
        }

        [Fact]
        public async Task GetEventByIdAsync_ShouldReturnEvent_WhenIdIsValid()
        {
            // Arrange
            var id = 1;
            var calendarEvent = new CalendarEvent { Id = id };
            var dto = new ReadCalendarEventDto { Id = id };

            _mockDbContext.Setup(db => db.CalendarEvents.FindAsync(id)).ReturnsAsync(calendarEvent);
            _mockMapper.Setup(m => m.Map<ReadCalendarEventDto>(calendarEvent)).Returns(dto);

            // Act
            var result = await _service.GetEventByIdAsync(id);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(dto, result.Data);
        }

        [Fact]
        public async Task CreateEventAsync_ShouldReturnError_WhenDtoIsNull()
        {
            // Act
            var result = await _service.CreateEventAsync(null);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Error creating event.", result.Error);
        }

        [Fact]
        public async Task CreateEventAsync_ShouldCreateEvent_WhenValidDtoIsProvided()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TaskDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var dbContext = new TaskDbContext(options);
            var logger = new Mock<ILoggerService>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<CalendarEventProfile>()).CreateMapper();
            var service = new CalendarEventService(dbContext, mapper, logger.Object);

            var dto = new CreateCalendarEventDto { UserId = "test-user", Title = "Test Event" };

            // Act
            var result = await service.CreateEventAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(dto.Title, result.Data.Title);
        }


        [Fact]
        public async Task DeleteEventAsync_ShouldReturnError_WhenIdIsInvalid()
        {
            // Act
            var result = await _service.DeleteEventAsync(0);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Error deleting event.", result.Error);
        }

        [Fact]
        public async Task DeleteEventAsync_ShouldDeleteEvent_WhenIdIsValid()
        {
            // Arrange
            var id = 1;
            var calendarEvent = new CalendarEvent { Id = id };

            // Mock DbSet
            var mockSet = new Mock<DbSet<CalendarEvent>>();
            var data = new List<CalendarEvent> { calendarEvent }.AsQueryable();
            
            mockSet.As<IQueryable<CalendarEvent>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CalendarEvent>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CalendarEvent>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CalendarEvent>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(ids => ValueTask.FromResult(data.FirstOrDefault(d => d.Id == (int)ids[0])));


            _mockDbContext.Setup(db => db.Set<CalendarEvent>()).Returns(mockSet.Object);
            _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteEventAsync(id);

            // Assert
            Assert.True(result.Success); // Forventet resultat
            Assert.True(result.Data); // Data skal også være sand
            mockSet.Verify(m => m.Remove(It.IsAny<CalendarEvent>()), Times.Once);
            _mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
        }



    }
}