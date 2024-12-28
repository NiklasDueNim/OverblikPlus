using Microsoft.AspNetCore.Mvc;
using UserMicroService.dto;
using UserMicroService.Services.Interfaces;
using FluentValidation;
using OverblikPlus.Shared.Interfaces;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<CreateUserDto> _createUserValidator;
        private readonly IValidator<UpdateUserDto> _updateUserValidator;
        private readonly ILoggerService _logger;

        public UserController(
            IUserService userService,
            IValidator<CreateUserDto> createUserValidator,
            IValidator<UpdateUserDto> updateUserValidator,
            ILoggerService logger)
        {
            _userService = userService;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
            _logger = logger;
        }

        // GET /api/userservice/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInfo($"GetUserById called with ID: {id}");

            var user = await _userService.GetUserById(id, "Admin");
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found");
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            _logger.LogInfo($"User with ID: {id} retrieved successfully");
            return Ok(user);
        }

        // GET /api/userservice/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            _logger.LogInfo("GetAllUsers called");

            var users = await _userService.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No users found");
                return NotFound(new { Message = "No users found." });
            }

            _logger.LogInfo("All users retrieved successfully");
            return Ok(users);
        }

        // POST /api/userservice
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto)
        {
            _logger.LogInfo("CreateUser called");

            var validationResult = await _createUserValidator.ValidateAsync(createUserDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for CreateUserDto");
                return BadRequest(validationResult.Errors);
            }

            var userId = await _userService.CreateUserAsync(createUserDto);
            _logger.LogInfo($"User with ID: {userId} created successfully");

            return CreatedAtAction(nameof(GetUserById), new { id = userId }, createUserDto);
        }

        // PUT /api/userservice/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            _logger.LogInfo($"UpdateUser called for ID: {id}");

            var validationResult = await _updateUserValidator.ValidateAsync(updateUserDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning($"Validation failed for UpdateUserDto with ID: {id}");
                return BadRequest(validationResult.Errors);
            }

            var user = await _userService.GetUserById(id, "Admin");
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found for update");
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            await _userService.UpdateUserAsync(id, updateUserDto);
            _logger.LogInfo($"User with ID: {id} updated successfully");

            return NoContent();
        }

        // DELETE /api/userservice/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            _logger.LogInfo($"DeleteUser called for ID: {id}");

            var user = await _userService.GetUserById(id, "Admin");
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found for deletion");
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            await _userService.DeleteUserAsync(id);
            _logger.LogInfo($"User with ID: {id} deleted successfully");

            return NoContent();
        }
    }
}
