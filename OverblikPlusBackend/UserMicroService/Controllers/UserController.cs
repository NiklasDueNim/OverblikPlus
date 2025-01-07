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
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _createUserValidator = createUserValidator ?? throw new ArgumentNullException(nameof(createUserValidator));
            _updateUserValidator = updateUserValidator ?? throw new ArgumentNullException(nameof(updateUserValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInfo($"GetUserById called with ID: {id}");

            var result = await _userService.GetUserById(id, "Admin");
            if (!result.Success)
            {
                _logger.LogWarning($"User with ID: {id} not found");
                return NotFound(new { Message = result.Error });
            }

            _logger.LogInfo($"User with ID: {id} retrieved successfully");
            return Ok(result.Data);
        }

      
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            _logger.LogInfo("GetAllUsers called");

            var result = await _userService.GetAllUsersAsync();
            if (!result.Success || !result.Data.Any())
            {
                _logger.LogWarning("No users found");
                return NotFound(new { Message = "No users found." });
            }

            _logger.LogInfo("All users retrieved successfully");
            return Ok(result.Data);
        }

        
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

            var result = await _userService.CreateUserAsync(createUserDto);
            if (!result.Success)
            {
                _logger.LogWarning($"Failed to create user: {result.Error}");
                return BadRequest(new { Message = result.Error });
            }

            _logger.LogInfo($"User with ID: {result.Data} created successfully");
            return CreatedAtAction(nameof(GetUserById), new { id = result.Data }, createUserDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            if (updateUserDto == null)
            {
                return BadRequest("UpdateUserDto is required.");
            }

            var validationResult = await _updateUserValidator.ValidateAsync(updateUserDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userResult = await _userService.GetUserById(id, "Admin");
            if (!userResult.Success)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            var updateResult = await _userService.UpdateUserAsync(id, updateUserDto);
            if (!updateResult.Success)
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            var userResult = await _userService.GetUserById(id, "Admin");
            if (!userResult.Success)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            var deleteResult = await _userService.DeleteUserAsync(id);
            if (!deleteResult.Success)
            {
                return StatusCode(500, "An error occurred while deleting the user.");
            }

            return NoContent();
        }
    }
}