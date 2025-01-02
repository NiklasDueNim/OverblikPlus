using Moq;
using OverblikPlus.Shared.Logging;
using Serilog;
using Xunit;

namespace OverblikPlus.Shared.Tests.Unittests
{
    public class LoggerServiceTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly LoggerService _loggerService;

        public LoggerServiceTests()
        {
            _mockLogger = new Mock<ILogger>();
            _loggerService = new LoggerService(_mockLogger.Object);
        }

        [Fact]
        public void LogInfo_ShouldCallInformation()
        {
            // Arrange
            var message = "Info message";

            // Act
            _loggerService.LogInfo(message);

            // Assert
            _mockLogger.Verify(x => x.Information(message), Times.Once);
        }

        [Fact]
        public void LogWarning_ShouldCallWarning()
        {
            // Arrange
            var message = "Warning message";

            // Act
            _loggerService.LogWarning(message);

            // Assert
            _mockLogger.Verify(x => x.Warning(message), Times.Once);
        }

        [Fact]
        public void LogError_ShouldCallError()
        {
            // Arrange
            var message = "Error message";
            var exception = new Exception("Test exception");

            // Act
            _loggerService.LogError(message, exception);

            // Assert
            _mockLogger.Verify(x => x.Error(message + " Exception: {Exception}", exception), Times.Once);
        }
    }
}