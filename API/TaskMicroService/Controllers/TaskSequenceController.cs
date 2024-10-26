using Microsoft.AspNetCore.Mvc;
using TaskMicroService;

[ApiController]
[Route("api/[controller]")]
public class TaskSequenceController : ControllerBase
{
    // Hent trin baseret på opgave-id
    [HttpGet("{taskId}")]
    public IActionResult GetTaskSequence(int taskId)
    {
        // Eksempeldata - disse kunne normalt komme fra en database
        var steps = new List<TaskStep>
        {
            new TaskStep { Id = 1, TaskId = taskId, StepNumber = 1, ImageUrl = "https://din-server.com/images/step1.jpg", Text = "Dette er trin 1 i opgaven." },
            new TaskStep { Id = 2, TaskId = taskId, StepNumber = 2, ImageUrl = "https://din-server.com/images/step2.jpg", Text = "Dette er trin 2 i opgaven." },
            new TaskStep { Id = 3, TaskId = taskId, StepNumber = 3, ImageUrl = "https://din-server.com/images/step3.jpg", Text = "Dette er trin 3 i opgaven." }
        };

        return Ok(steps);
    }
}