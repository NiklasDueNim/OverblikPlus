// using Microsoft.AspNetCore.Mvc;
//
// namespace UserMicroService.Controllers;
//
// [ApiController]
// [Route("/api/[controller]")]
// public class RelativeController : ControllerBase
// {
//
//     public RelativeController()
//     {
//         
//     }
//
//     [HttpGet("{userId}/tasks-for-day")]
//     public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksForDay(string userId, [FromQuery] DateTime date)
//     {
//         try
//         {
//             var tasks = await _relativeService.GetTasksForDay(userId, date);
//             return Ok(tasks);
//         }
//         catch (Exception ex)
//         {
//             return BadRequest(new { Message = ex.Message });
//         }
//     }
// }