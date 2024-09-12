using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {

        [HttpGet("today")]
        public IActionResult GetTodaysTasks()
        { 
            var tasks = new List<string> { "Task 1", "Task 2", "Task 3"};
            return Ok(tasks);
        }

    }

