using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.dto;
using UserMicroService.Services.Interfaces;
using UserMicroService.Common;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly ILoggerService _logger;

        public AuthController(
            IAuthService authService,
            IValidator<LoginDto> loginValidator,
            IValidator<RegisterDto> registerValidator,
            ILoggerService logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _loginValidator = loginValidator ?? throw new ArgumentNullException(nameof(loginValidator));
            _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var validationResult = _loginValidator.Validate(loginDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Login validation failed.");
                return BadRequest(validationResult.Errors);
            }

            var result = await _authService.LoginAsync(loginDto);
            if (!result.Success)
            {
                _logger.LogWarning($"Login failed for user {loginDto.Email}");
                return Unauthorized(result.Error);
            }

            _logger.LogInfo($"User {loginDto.Email} logged in successfully.");
            return Ok(result.Data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var validationResult = _registerValidator.Validate(registerDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Registration validation failed.");
                return BadRequest(validationResult.Errors);
            }

            var result = await _authService.RegisterAsync(registerDto);
            if (!result.Success)
            {
                _logger.LogWarning($"Registration failed for user {registerDto.Email}");
                return BadRequest(result.Error);
            }

            _logger.LogInfo($"User {registerDto.Email} registered successfully.");
            return Ok("User registered successfully.");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Refresh token is missing.");
                return BadRequest("Refresh token is required.");
            }

            var result = await _authService.RefreshTokenAsync(token);
            if (!result.Success)
            {
                _logger.LogWarning("Failed to refresh token.");
                return Unauthorized(result.Error);
            }

            _logger.LogInfo("Token refreshed successfully.");
            return Ok(new { Token = result.Data });
        }
    }
}