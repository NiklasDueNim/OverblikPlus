using Microsoft.AspNetCore.Mvc;
using TaskMicroService.Services;
using TaskMicroService.dto;
using AutoMapper;
using TaskMicroService.Entities;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskStepController : ControllerBase
    {
        private readonly ITaskStepService _taskStepService;
        private readonly IMapper _mapper;

        public TaskStepController(ITaskStepService taskStepService, IMapper mapper)
        {
            _taskStepService = taskStepService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskStep([FromBody] TaskStepDto stepDto)
        {
            // Mapper TaskStepDto til TaskStep
            var step = _mapper.Map<TaskStep>(stepDto);

            // Opretter TaskStep ved at sende den mapperede enhed til servicen
            var createdStepNumber = await _taskStepService.CreateTaskStep(step);

            // Returnerer DTO og location i CreatedAtAction
            return CreatedAtAction(nameof(GetTaskStep), new { taskId = step.TaskId, stepNumber = createdStepNumber }, stepDto);
        }

        [HttpGet("{taskId}/step/{stepNumber}")]
        public async Task<IActionResult> GetTaskStep(int taskId, int stepNumber)
        {
            var stepData = await _taskStepService.GetTaskStep(taskId, stepNumber);
            if (stepData == null)
            {
                return NotFound();
            }

            // Mapper til DTO for at returnere kun nødvendige data
            var stepDto = _mapper.Map<TaskStepDto>(stepData);
            return Ok(stepDto);
        }

        [HttpGet("{taskId}/steps")]
        public async Task<IActionResult> GetAllStepsForTask(int taskId)
        {
            var steps = await _taskStepService.GetAllStepsForTask(taskId);
            
            // Mapper listen af TaskSteps til DTO'er
            var stepDtos = _mapper.Map<List<TaskStepDto>>(steps);
            return Ok(stepDtos);
        }

        [HttpPut("{taskId}/step/{stepNumber}")]
        public async Task<IActionResult> UpdateTaskStep(int taskId, int stepNumber, [FromBody] TaskStepDto updatedStepDto)
        {
            // Mapper DTO til TaskStep enhed
            var updatedStep = _mapper.Map<TaskStep>(updatedStepDto);
            await _taskStepService.UpdateTaskStep(taskId, stepNumber, updatedStepDto);
            
            return NoContent();
        }

        [HttpDelete("{taskId}/step/{stepNumber}")]
        public async Task<IActionResult> DeleteTaskStep(int taskId, int stepNumber)
        {
            await _taskStepService.DeleteTaskStep(taskId, stepNumber);
            return NoContent();
        }
    }
}
