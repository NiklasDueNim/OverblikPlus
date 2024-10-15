using System.Collections.Generic;
using API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ITaskService _taskService;

    public UserController(ITaskService taskService)
    {
        _taskService = taskService;
    }
    
    [HttpGet("id")]
    public IActionResult GetUserById()
    {
        var liste = new List<int>();
        liste = [1, 2, 3];
        return Ok(liste);
    }
}