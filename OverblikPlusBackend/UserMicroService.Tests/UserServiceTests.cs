using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserMicroService.Controllers;
using UserMicroService.DataAccess;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_ShouldReturnUserIdAsync()
    {
        // Sæt krypteringsnøgle til testen
        EncryptionHelper.SetEncryptionKey("d0aeXMamTP1Atr3q8VmsabazTVbeBzTF");

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        var context = new UserDbContext(options);

        // Mock IMapper
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<UserEntity>(It.IsAny<CreateUserDto>()))
            .Returns((CreateUserDto dto) => 
                new UserEntity 
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    CPRNumber = dto.CPRNumber,
                    MedicationDetails = dto.MedicationDetails,
                    Role = dto.Role,               // Tilføj dette
                    Username = dto.Username
                });

        var service = new UserService(context, mockMapper.Object);

        var createUserDto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            CPRNumber = "12345678-1234",
            MedicationDetails = "Ingen",
            Role = "User",
            Username = "johndoe"
        };

        // Act
        var result = await service.CreateUserAsync(createUserDto);

        // Assert
        Assert.True(result > 0); // Ensure that a user ID is returned
        Assert.NotNull(await context.Users.FirstOrDefaultAsync(u => u.FirstName == "John")); // Ensure that the user was added to the database
    }


    
    [Fact]
    public void EncryptionHelper_ShouldEncryptAndDecryptSuccessfully()
    {
        // Arrange
        string encryptionKey = "d0aeXMamTP1Atr3q8VmsabazTVbeBzTF"; // 32-tegns nøgle
        EncryptionHelper.SetEncryptionKey(encryptionKey);

        string originalText = "TestData";

        // Act
        string encryptedText = EncryptionHelper.Encrypt(originalText);
        string decryptedText = EncryptionHelper.Decrypt(encryptedText);

        // Assert
        Assert.Equal(originalText, decryptedText);
    }
    
    
    [Fact]
    public async Task CreateUser_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createUserDto = new CreateUserDto { FirstName = "Test", LastName = "User", CPRNumber = "12345678", MedicationDetails = "None", Role = "User", Username = "testuser" };
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(service => service.CreateUserAsync(It.IsAny<CreateUserDto>())).ReturnsAsync(1);
        var controller = new UserServiceController(mockUserService.Object, Mock.Of<ILogger<UserServiceController>>());

        // Act
        var result = await controller.CreateUserAsync(createUserDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }



}