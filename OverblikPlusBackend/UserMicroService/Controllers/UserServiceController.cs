using Microsoft.AspNetCore.Mvc;
using UserMicroService.dto;
using UserMicroService.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserServiceController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserServiceController> _logger;

        public UserServiceController(IUserService userService, ILogger<UserServiceController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET /api/userservice/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation($"GetUserById called with ID: {id}");

            var user = await _userService.GetUserById(id, "Admin");

            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found");
                return NotFound();
            }

            _logger.LogInformation($"User with ID: {id} retrieved successfully");
            return Ok(user);
        }

        // GET /api/userservice/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            _logger.LogInformation("GetAllUsers called");

            var users = await _userService.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No users found");
                return NotFound();
            }

            _logger.LogInformation("All users retrieved successfully");
            return Ok(users);
        }

        // POST /api/userservice
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid CreateUserDto received");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("CreateUser called");

            var userId = await _userService.CreateUserAsync(createUserDto);
            _logger.LogInformation($"User with ID: {userId} created successfully");
            
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, createUserDto );
        }

        // PUT /api/userservice/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid UpdateUserDto received for user ID: {id}");
                return BadRequest(ModelState);
            }

            var user = await _userService.GetUserById(id, "Admin"); // Assuming only admins can update
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found for update");
                return NotFound();
            }

            _logger.LogInformation($"UpdateUser called for user ID: {id}");
            await _userService.UpdateUserAsync(id, updateUserDto);
            _logger.LogInformation($"User with ID: {id} updated successfully");

            return NoContent();
        }

        // DELETE /api/userservice/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            var user = await _userService.GetUserById(id, "Admin"); // Assuming only admins can delete
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found for deletion");
                return NotFound();
            }

            _logger.LogInformation($"DeleteUser called for user ID: {id}");
            await _userService.DeleteUserAsync(id);
            _logger.LogInformation($"User with ID: {id} deleted successfully");

            return NoContent();
        }
    }
}
