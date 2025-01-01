using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using System;
using System.Threading.Tasks;
using UserMicroService.dto;
using UserMicroService.Services.Interfaces;

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

            try
            {
                var (token, refreshToken) = await _authService.LoginAsync(loginDto);
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning($"Login failed for user {loginDto.Email}");
                    return Unauthorized("Invalid login credentials.");
                }

                _logger.LogInfo($"User {loginDto.Email} logged in successfully.");
                return Ok(new { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred during login.", ex);
                return StatusCode(500, "An error occurred during login.");
            }
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

            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (!result.Success)
                {
                    _logger.LogWarning($"Registration failed for user {registerDto.Email}");
                    return BadRequest(result.Errors);
                }

                _logger.LogInfo($"User {registerDto.Email} registered successfully.");
                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred during registration.", ex);
                return StatusCode(500, "An error occurred during registration.");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Refresh token is missing.");
                return BadRequest("Refresh token is required.");
            }

            try
            {
                var newToken = await _authService.RefreshTokenAsync(token);
                if (string.IsNullOrEmpty(newToken))
                {
                    _logger.LogWarning("Failed to refresh token.");
                    return Unauthorized("Failed to refresh token.");
                }

                _logger.LogInfo("Token refreshed successfully.");
                return Ok(new { Token = newToken });
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred during token refresh.", ex);
                return StatusCode(500, "An error occurred during token refresh.");
            }
        }
    }
}