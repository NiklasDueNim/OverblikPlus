using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using UserMicroService.DataAccess;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Services;

namespace UserMicroService.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly UserDbContext _dbContext;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
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

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _dbContext = new UserDbContext(options);

        _authService = new AuthService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockConfiguration.Object,
            _dbContext
        );
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenLoginIsSuccessful()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        var user = new ApplicationUser { Id = "1", Email = loginDto.Email, UserName = loginDto.Email, Role = "User"};

        _mockSignInManager
            .Setup(s => s.PasswordSignInAsync(loginDto.Email, loginDto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager
            .Setup(u => u.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Item1));
        Assert.False(string.IsNullOrEmpty(result.Item2));
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

        _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), registerDto.Role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Errors);
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
        Assert.True(result);
    }

    [Fact]
    public async Task LogoutAsync_CallsSignOut()
    {
        // Arrange
        _mockSignInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }
}
