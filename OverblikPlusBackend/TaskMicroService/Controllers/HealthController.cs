using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.DataAccess;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TaskDbContext _context;
        private readonly ILoggerService _logger;

        public HealthController(TaskDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", message = "Task API is running", timestamp = DateTime.UtcNow });
        }

        [HttpGet("db")]
        public async Task<IActionResult> DatabaseHealth()
        {
            try
            {
                _logger.LogInfo("Testing database connection...");
                
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database", new Exception("Database connection failed"));
                    return StatusCode(500, new { 
                        status = "ERROR", 
                        message = "Cannot connect to database",
                        timestamp = DateTime.UtcNow 
                    });
                }

                // Test a simple query
                var taskCount = await _context.Tasks.CountAsync();
                
                _logger.LogInfo($"Database connection successful. Task count: {taskCount}");
                
                return Ok(new { 
                    status = "OK", 
                    message = "Database connection successful",
                    taskCount = taskCount,
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database health check failed: {ex.Message}", ex);
                return StatusCode(500, new { 
                    status = "ERROR", 
                    message = $"Database error: {ex.Message}",
                    errorType = ex.GetType().Name,
                    timestamp = DateTime.UtcNow 
                });
            }
        }
    }
}
