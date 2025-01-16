using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILoggerService _logger;
        private readonly IValidator<CreateTaskDto> _createTaskValidator;
        private readonly IValidator<UpdateTaskDto> _updateTaskValidator;

        public TaskController(
            ITaskService taskService,
            ILoggerService logger,
            IValidator<CreateTaskDto> createTaskValidator,
            IValidator<UpdateTaskDto> updateTaskValidator)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createTaskValidator = createTaskValidator ?? throw new ArgumentNullException(nameof(createTaskValidator));
            _updateTaskValidator = updateTaskValidator ?? throw new ArgumentNullException(nameof(updateTaskValidator));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            _logger.LogInfo($"Getting task by ID: {id}");
            var result = await _taskService.GetTaskById(id);
            if (!result.Success)
            {
                _logger.LogWarning($"Task with ID {id} not found.");
                return NotFound(result);
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            _logger.LogInfo("Getting all tasks.");
            var result = await _taskService.GetAllTasks();
            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve tasks.");
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId(string userId)
        {
            _logger.LogInfo($"Getting tasks for user ID: {userId}");
            var result = await _taskService.GetTasksByUserId(userId);
            if (!result.Success)
            {
                _logger.LogWarning($"Failed to retrieve tasks for user ID {userId}.");
                return BadRequest(result);
            }

            return Ok(result);
        }
        

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var validationResult = await _createTaskValidator.ValidateAsync(createTaskDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for creating task.");
                return BadRequest(Result<object>.ErrorResult("Validation failed"));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole == "User")
                createTaskDto.UserId = currentUserId;

            _logger.LogInfo("Creating a new task.");
            var result = await _taskService.CreateTask(createTaskDto);
            if (!result.Success)
            {
                _logger.LogError("Failed to create task.", new Exception(result.Error));
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetTaskById), new { id = result.Data }, result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var validationResult = await _updateTaskValidator.ValidateAsync(updateTaskDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for updating task.");
                return BadRequest(Result<object>.ErrorResult("Validation failed"));
            }

            _logger.LogInfo($"Updating task with ID: {id}");
            var result = await _taskService.UpdateTask(id, updateTaskDto);
            if (!result.Success)
            {
                _logger.LogError($"Failed to update task with ID {id}.", new Exception(result.Error));
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            _logger.LogInfo($"Deleting task with ID: {id}");
            var result = await _taskService.DeleteTask(id);
            if (!result.Success)
            {
                _logger.LogError($"Failed to delete task with ID {id}.", new Exception(result.Error));
                return NotFound(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{taskId}/complete")]
        public async Task<IActionResult> MarkTaskAsCompleted(int taskId)
        {
            if (taskId <= 0)
            {
                _logger.LogWarning("Invalid task ID for marking as completed.");
                return BadRequest(Result<object>.ErrorResult("Invalid task ID."));
            }

            _logger.LogInfo($"Marking task with ID {taskId} as completed.");
            var result = await _taskService.MarkTaskAsCompleted(taskId);
            if (!result.Success)
            {
                _logger.LogError($"Failed to mark task with ID {taskId} as completed.", new Exception(result.Error));
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("user/{userId}/tasks-for-day")]
        public async Task<IActionResult> GetTasksForDay(string userId, [FromQuery] DateTime date)
        {
            _logger.LogInfo($"Getting tasks for user ID {userId} for date {date.ToShortDateString()}.");
            var result = await _taskService.GetTasksForDay(userId, date);
            if (!result.Success)
            {
                _logger.LogWarning($"Failed to retrieve tasks for user ID {userId} for date {date.ToShortDateString()}.");
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}