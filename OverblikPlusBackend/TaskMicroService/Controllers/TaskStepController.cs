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
            _taskStepService = taskStepService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStepsForTask(int taskId)
        {
            var steps = await _taskStepService.GetStepsForTask(taskId);
            return Ok(steps);
        }

        [HttpGet("{stepId}")]
        public async Task<IActionResult> GetTaskStep(int taskId, int stepId)
        {
            var step = await _taskStepService.GetTaskStep(taskId, stepId);
            if (step == null)
                return NotFound();

            return Ok(step);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskStep(int taskId, [FromBody] CreateTaskStepDto createStepDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            createStepDto.TaskId = taskId;
            var stepId = await _taskStepService.CreateTaskStep(createStepDto);
            return CreatedAtAction(nameof(GetTaskStep), new { taskId = taskId, stepId = stepId }, null);
        }


        [HttpPut("{stepId}")]
        public async Task<IActionResult> UpdateTaskStep(int taskId, int stepId, [FromBody] UpdateTaskStepDto updateStepDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingStep = await _taskStepService.GetTaskStep(taskId, stepId);
            if (existingStep == null)
                return NotFound();

            await _taskStepService.UpdateTaskStep(taskId, stepId, updateStepDto);
            return NoContent();
        }

        [HttpDelete("{stepId}")]
        public async Task<IActionResult> DeleteTaskStep(int taskId, int stepId)
        {
            Console.WriteLine($"Deleting step {stepId} for task {taskId}");
            var existingStep = await _taskStepService.GetTaskStep(taskId, stepId);
            if (existingStep == null)
                return NotFound();

            await _taskStepService.DeleteTaskStep(taskId, stepId);
            return NoContent();
        }
    }
}
