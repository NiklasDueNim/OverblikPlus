using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMicroService.Services;
using TaskMicroService.dto;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        // [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var taskDto = await _taskService.GetTaskById(id);
            if (taskDto == null)
                return NotFound();

            var currentUserRole = GetCurrentUserRole();
            var currentUserId = GetCurrentUserId();

            if (taskDto.UserId != currentUserId && currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Forbid();
            }

            return Ok(taskDto);
        }

        
        // [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasks();
            return Ok(tasks);
        }

        
        // [Authorize]
        [HttpGet("user-tasks")]
        public async Task<IActionResult> GetTasksForLoggedInUser()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var tasks = await _taskService.GetTasksByUserId(currentUserId);
            return Ok(tasks);
        }

        
        // [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId(string userId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserId != userId && currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Forbid();
            }

            var tasks = await _taskService.GetTasksByUserId(userId);
            return Ok(tasks);
        }

        
        // [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid");
                return BadRequest(ModelState);
            }

            try
            {
                var taskId = await _taskService.CreateTask(createTaskDto);
                Console.WriteLine($"Task created with ID: {taskId}");
                return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating task: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTask = await _taskService.GetTaskById(id);
            if (existingTask == null)
                return NotFound();

            await _taskService.UpdateTask(id, updateTaskDto);
            return NoContent();
        }

        // [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var existingTask = await _taskService.GetTaskById(id);
            if (existingTask == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (existingTask.UserId != currentUserId && currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Forbid();
            }

            await _taskService.DeleteTask(id);
            return NoContent();
        }
    }
}
