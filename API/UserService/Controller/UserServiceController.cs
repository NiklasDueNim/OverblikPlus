using Microsoft.AspNetCore.Mvc;
using UserService.dto;

namespace UserService.Controller;


[ApiController]
[Route("api/[controller]")]
public class UserServiceController : ControllerBase
{

    public UserServiceController()
    {
        
    }


    [HttpGet("id")]
    public IActionResult GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        throw new NotImplementedException();
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