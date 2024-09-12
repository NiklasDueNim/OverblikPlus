using API.Dto;
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

        [HttpPost]
        public IActionResult CreateTask([FromBody] TaskDto taskDto)
        {
            return Ok("Opgave oprettet");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, [FromBody] TaskDto taskDto)
        {
            return Ok($"Opgave med ID {id} er blevet opdateret");
        }

        [HttpDelete ("{id}")]
        public IActionResult DeleteTask(int id)
        {
            return Ok($"Opgave med ID: {id} er slettet");
        }

    }

