using OverblikPlus.Shared.Interfaces;
using Serilog;

namespace OverblikPlus.Shared.Logging
{
    public class LoggerService : ILoggerService
    {
        private readonly Serilog.ILogger _logger;

        public LoggerService(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message)
        {
            _logger.Information(message);
        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.Error(message + " Exception: {Exception}", ex);
        }
    }
}