using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using UserService.dto;
using UserService.Service;

namespace UserService.Controller;


[ApiController]
[Route("api/[controller]")]
public class UserServiceController : ControllerBase
{
    private readonly IUserService _userService;

    public UserServiceController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpGet("id")]
    public IActionResult GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var users = _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserDto createUserDto)
    {
        throw new NotImplementedException(); 
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser()
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser()
    {
        throw new NotImplementedException();
    }

}