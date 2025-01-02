using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserMicroService.DataAccess;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Services;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.Validators.Auth;
using UserMicroService.Common;

namespace UserMicroService.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly UserDbContext _dbContext;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            new IUserValidator<ApplicationUser>[0],
            new IPasswordValidator<ApplicationUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object
        );

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null
        );

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("TestSecretKey12345678901234567890");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _mockLogger = new Mock<ILoggerService>();

        _registerValidator = new RegisterDtoValidator();

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _dbContext = new UserDbContext(options);

        _authService = new AuthService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockConfiguration.Object,
            _dbContext,
            _mockLogger.Object,
            _registerValidator
        );
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_WhenRegistrationIsValid()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "Password123!",
            Role = "User"
        };

        var user = new ApplicationUser
        {
            Id = "1",
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email
        };

        _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), registerDto.Role))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(u => u.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task ChangePasswordAsync_ChangesPassword_WhenInputIsValid()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            Email = "test@example.com",
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        var user = new ApplicationUser { Id = "1", Email = changePasswordDto.Email };

        _mockUserManager.Setup(u => u.FindByEmailAsync(changePasswordDto.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(u => u.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.ChangePasswordAsync(changePasswordDto);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task LogoutAsync_CallsSignOut()
    {
        // Arrange
        _mockSignInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync();

        // Assert
        Assert.True(result.Success);
        _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }
}