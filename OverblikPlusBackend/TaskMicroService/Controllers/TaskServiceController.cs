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

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var taskDto = await _taskService.GetTaskById(id);
            if (taskDto == null)
                return NotFound();

            return Ok(taskDto);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasks();
            return Ok(tasks);
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-tasks")]
        public async Task<IActionResult> GetTasksForLoggedInUser()
        {
            var userId = User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var tasks = await _taskService.GetTasksByUserId(userId);
            return Ok(tasks);
        }

        [Authorize(Roles = "User")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId(string userId)
        {
            var tasks = await _taskService.GetTasksByUserId(userId);
            return Ok(tasks);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var taskId = await _taskService.CreateTask(createTaskDto);
                return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating task: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Staff")]
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

        [Authorize(Roles = "Admin,Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var existingTask = await _taskService.GetTaskById(id);
            if (existingTask == null)
                return NotFound();

            await _taskService.DeleteTask(id);
            return NoContent();
        }
    }
}
