using Microsoft.AspNetCore.Mvc;
using TaskMicroService.Services;
using TaskMicroService.Entities;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskStepController : ControllerBase
    {
        private readonly ITaskStepService _taskStepService;

        public TaskStepController(ITaskStepService taskStepService)
        {
            _taskStepService = taskStepService;
        }

        [HttpGet("{taskId}/step/{stepNumber}")]
        public async Task<IActionResult> GetTaskStep(int taskId, int stepNumber)
        {
            var stepData = await _taskStepService.GetTaskStep(taskId, stepNumber);
            if (stepData == null)
            {
                return NotFound();
            }
            return Ok(stepData);
        }

        [HttpGet("{taskId}/steps")]
        public async Task<IActionResult> GetAllStepsForTask(int taskId)
        {
            var steps = await _taskStepService.GetAllStepsForTask(taskId);
            return Ok(steps);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskStep([FromBody] TaskStep step)
        {
            var stepNumber = await _taskStepService.CreateTaskStep(step);
            return CreatedAtAction(nameof(GetTaskStep), new { taskId = step.TaskId, stepNumber = step.StepNumber }, step);
        }

        [HttpPut("{taskId}/step/{stepNumber}")]
        public async Task<IActionResult> UpdateTaskStep(int taskId, int stepNumber, [FromBody] TaskStep updatedStep)
        {
            await _taskStepService.UpdateTaskStep(taskId, stepNumber, updatedStep);
            return NoContent();
        }

        [HttpDelete("{taskId}/step/{stepNumber}")]
        public async Task<IActionResult> DeleteTaskStep(int taskId, int stepNumber)
        {
            await _taskStepService.DeleteTaskStep(taskId, stepNumber);
            return NoContent();
        }
        
        // [HttpGet("{taskId}/steps")]
        // public async Task<IActionResult> GetTaskSteps(int taskId)
        // {
        //     var steps = await _taskService.GetStepsForTask(taskId);
        //     if (steps == null || !steps.Any())
        //         return NotFound("Ingen trin fundet for denne opgave.");
        //
        //     return Ok(steps);
        // }
        //
        

    }
}