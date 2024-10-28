using Microsoft.AspNetCore.Mvc;
using TaskMicroService.Services;
using TaskMicroService.dto;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/steps")]
    public class TaskStepController : ControllerBase
    {
        private readonly ITaskStepService _taskStepService;

        public TaskStepController(ITaskStepService taskStepService)
        {
            _taskStepService = taskStepService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStepsForTask(int taskId)
        {
            var steps = await _taskStepService.GetStepsForTask(taskId);
            return Ok(steps);
        }

        [HttpGet("{stepNumber}")]
        public async Task<IActionResult> GetTaskStep(int taskId, int stepNumber)
        {
            var step = await _taskStepService.GetTaskStep(taskId, stepNumber);
            if (step == null)
                return NotFound();

            return Ok(step);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskStep(int taskId, [FromBody] CreateTaskStepDto createStepDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            createStepDto.TaskId = taskId; // Indstil TaskId i DTO'en
            var stepNumber = await _taskStepService.CreateTaskStep(createStepDto);
            return CreatedAtAction(nameof(GetTaskStep), new { taskId = taskId, stepNumber = stepNumber }, null);
        }


        [HttpPut("{stepNumber}")]
        public async Task<IActionResult> UpdateTaskStep(int taskId, int stepNumber, [FromBody] UpdateTaskStepDto updateStepDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingStep = await _taskStepService.GetTaskStep(taskId, stepNumber);
            if (existingStep == null)
                return NotFound();

            await _taskStepService.UpdateTaskStep(taskId, stepNumber, updateStepDto);
            return NoContent();
        }

        [HttpDelete("{stepNumber}")]
        public async Task<IActionResult> DeleteTaskStep(int taskId, int stepNumber)
        {
            var existingStep = await _taskStepService.GetTaskStep(taskId, stepNumber);
            if (existingStep == null)
                return NotFound();

            await _taskStepService.DeleteTaskStep(taskId, stepNumber);
            return NoContent();
        }
    }
}
