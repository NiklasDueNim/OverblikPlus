using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OverblikPlus.Shared.Interfaces;
using System.Threading.Tasks;
using UserMicroService;
using UserMicroService.Controllers;
using UserMicroService.dto;
using UserMicroService.Services.Interfaces;
using Xunit;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IValidator<LoginDto>> _loginValidatorMock;
    private readonly Mock<IValidator<RegisterDto>> _registerValidatorMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loginValidatorMock = new Mock<IValidator<LoginDto>>();
        _registerValidatorMock = new Mock<IValidator<RegisterDto>>();
        _loggerMock = new Mock<ILoggerService>();
        _controller = new AuthController(
            _authServiceMock.Object,
            _loginValidatorMock.Object,
            _registerValidatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var loginDto = new LoginDto();
        _loginValidatorMock.Setup(v => v.Validate(loginDto)).Returns(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Email", "Email is required") }));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenLoginFails()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "password" };
        _loginValidatorMock.Setup(v => v.Validate(loginDto)).Returns(new FluentValidation.Results.ValidationResult());
        _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync((string.Empty, string.Empty));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid login credentials.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginSucceeds()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "password" };
        _loginValidatorMock.Setup(v => v.Validate(loginDto)).Returns(new FluentValidation.Results.ValidationResult());
        _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(("token", "refreshToken"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var registerDto = new RegisterDto();
        _registerValidatorMock.Setup(v => v.Validate(registerDto)).Returns(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Email", "Email is required") }));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Password = "password" };
        _registerValidatorMock.Setup(v => v.Validate(registerDto)).Returns(new FluentValidation.Results.ValidationResult());
        _authServiceMock.Setup(s => s.RegisterAsync(registerDto)).ReturnsAsync(new RegistrationResult { Success = false, Errors = new[] { "Error" } });

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Password = "password" };
        _registerValidatorMock.Setup(v => v.Validate(registerDto)).Returns(new FluentValidation.Results.ValidationResult());
        _authServiceMock.Setup(s => s.RegisterAsync(registerDto)).ReturnsAsync(new RegistrationResult { Success = true });

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User registered successfully.", okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ReturnsBadRequest_WhenTokenIsMissing()
    {
        // Act
        var result = await _controller.RefreshToken(string.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Refresh token is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshFails()
    {
        // Arrange
        var token = "invalidToken";
        _authServiceMock.Setup(s => s.RefreshTokenAsync(token)).ReturnsAsync(string.Empty);

        // Act
        var result = await _controller.RefreshToken(token);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Failed to refresh token.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenRefreshSucceeds()
    {
        // Arrange
        var token = "validToken";
        _authServiceMock.Setup(s => s.RefreshTokenAsync(token)).ReturnsAsync("newToken");

        // Act
        var result = await _controller.RefreshToken(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}