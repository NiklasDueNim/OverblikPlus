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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var taskDto = await _taskService.GetTaskById(id);
            if (taskDto == null)
                return NotFound();
            
            return Ok(taskDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasks();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var taskId = await _taskService.CreateTask(createTaskDto);
            return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, null);
        }

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
