using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMicroService.dto;
using TaskMicroService.Services.Interfaces;

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
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (currentUserId != userId)
            {
                return Forbid();
            }

            var tasks = await _taskService.GetTasksByUserId(userId);
            return Ok(tasks);
        }


        
        [Authorize]
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
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

                if (currentUserRole == "User")
                {
                    createTaskDto.UserId = currentUserId;
                }

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



        [Authorize]
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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var existingTask = await _taskService.GetTaskById(id);
            if (existingTask == null)
                return NotFound();

            await _taskService.DeleteTask(id);
            return NoContent();
        }
        
        [HttpPut("{taskId}/complete")]
        public async Task<IActionResult> MarkTaskAsCompleted(int taskId)
        {
            await _taskService.MarkTaskAsCompleted(taskId);
            return NoContent();
        }
        
        [HttpGet("user/{userId}/tasks-for-day")]
        public async Task<ActionResult<IEnumerable<ReadTaskDto>>> GetTasksForDay(string userId, [FromQuery] DateTime date)
        {
            try
            {
                var tasks = await _taskService.GetTasksForDay(userId, date);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
