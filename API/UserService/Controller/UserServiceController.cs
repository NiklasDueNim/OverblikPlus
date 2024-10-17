using Microsoft.AspNetCore.Mvc;

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
  
}