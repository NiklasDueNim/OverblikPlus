using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMicroService.Common;
using TaskMicroService.dto;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;
        private readonly IValidator<CreateTaskDto> _createTaskValidator;
        private readonly IValidator<UpdateTaskDto> _updateTaskValidator;

        public TaskController(
            ITaskService taskService, 
            ILogger<TaskController> logger,
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
            var task = await _taskService.GetTaskById(id);
            if (task == null)
                return Ok(Result<ReadTaskDto>.ErrorResult("Task not found"));

            return Ok(Result<ReadTaskDto>.SuccessResult(task));
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasks();
            return Ok(Result<IEnumerable<ReadTaskDto>>.SuccessResult(tasks));
        }

        [Authorize(Roles = "User")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId != userId)
                return Forbid();

            var tasks = await _taskService.GetTasksByUserId(userId);
            return Ok(Result<IEnumerable<ReadTaskDto>>.SuccessResult(tasks));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var validationResult = _createTaskValidator.Validate(createTaskDto);
            if (!validationResult.IsValid)
                return BadRequest(Result<object>.ErrorResult("Validation failed"));

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole == "User")
                createTaskDto.UserId = currentUserId;

            var taskId = await _taskService.CreateTask(createTaskDto);
            return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, Result<int>.SuccessResult(taskId));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var validationResult = _updateTaskValidator.Validate(updateTaskDto);
            if (!validationResult.IsValid)
                return BadRequest(Result<object>.ErrorResult("Validation failed"));

            var existingTask = await _taskService.GetTaskById(id);
            if (existingTask == null)
                return NotFound(Result<object>.ErrorResult("Task not found"));

            await _taskService.UpdateTask(id, updateTaskDto);
            return Ok(Result<object>.SuccessResult(null));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var existingTask = await _taskService.GetTaskById(id);

            if (existingTask == null)
            {
                _logger.LogWarning("Task with ID {Id} not found.", id);
                return NotFound(Result<object>.ErrorResult("Task not found"));
            }

            await _taskService.DeleteTask(id);
            return Ok(Result<object>.SuccessResult(null));
        }

        [Authorize]
        [HttpPut("{taskId}/complete")]
        public async Task<IActionResult> MarkTaskAsCompleted(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(Result<object>.ErrorResult("Invalid task ID."));

            await _taskService.MarkTaskAsCompleted(taskId);
            return Ok(Result<object>.SuccessResult(null));
        }

        [Authorize]
        [HttpGet("user/{userId}/tasks-for-day")]
        public async Task<IActionResult> GetTasksForDay(string userId, [FromQuery] DateTime date)
        {
            var tasks = await _taskService.GetTasksForDay(userId, date);
            return Ok(Result<IEnumerable<ReadTaskDto>>.SuccessResult(tasks));
        }
    }
}
