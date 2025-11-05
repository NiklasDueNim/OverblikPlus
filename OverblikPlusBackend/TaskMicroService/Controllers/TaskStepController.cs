using Microsoft.AspNetCore.Mvc;
using TaskMicroService.dtos.TaskStep;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/steps")]
    public class TaskStepController : ControllerBase
    {
        private readonly ITaskStepService _taskStepService;

        public TaskStepController(ITaskStepService taskStepService)
        {
            _taskStepService = taskStepService ?? throw new ArgumentNullException(nameof(taskStepService));
        }

        [HttpGet]
        public async Task<IActionResult> GetStepsForTask(int taskId)
        {
            var result = await _taskStepService.GetStepsForTask(taskId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{stepId}")]
        public async Task<IActionResult> GetTaskStep(int taskId, int stepId)
        {
            var result = await _taskStepService.GetTaskStep(taskId, stepId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskStep(int taskId, [FromBody] CreateTaskStepDto createStepDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            createStepDto.TaskId = taskId;
            var result = await _taskStepService.CreateTaskStep(createStepDto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetTaskStep), new { taskId = taskId, stepId = result.Data }, result);
        }

        [HttpPut("{stepId}")]
        public async Task<IActionResult> UpdateTaskStep(int taskId, int stepId, [FromBody] UpdateTaskStepDto updateStepDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskStepService.UpdateTaskStep(taskId, stepId, updateStepDto);
            if (!result.Success)
                return NotFound(result);

            return NoContent();
        }

        [HttpDelete("{stepId}")]
        public async Task<IActionResult> DeleteTaskStep(int taskId, int stepId)
        {
            var result = await _taskStepService.DeleteTaskStep(taskId, stepId);
            if (!result.Success)
                return NotFound(result);

            return NoContent();
        }
    }
}
