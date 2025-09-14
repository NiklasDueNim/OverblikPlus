using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserMicroService.DataAccess;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Services.Interfaces;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserMicroService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _dbContext;
        private readonly ILoggerService _logger;
        private readonly IValidator<RegisterDto> _registerDtoValidator;
        private readonly IMapper _mapper;

        public AuthService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           RoleManager<IdentityRole> roleManager,
                           IConfiguration configuration,
                           UserDbContext dbContext,
                           ILoggerService logger,
                           IValidator<RegisterDto> registerDtoValidator, IMapper  mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dbContext = dbContext;
            _logger = logger;
            _registerDtoValidator = registerDtoValidator;
            _mapper = mapper;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, false, false);
                if (!result.Succeeded) return Result<LoginResponseDto>.ErrorResult("Invalid login attempt.");

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null) return Result<LoginResponseDto>.ErrorResult("User not found.");
                
               

                var jwtToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken(user.Id);
                await SaveRefreshTokenAsync(refreshToken);
                
                var userDto = _mapper.Map<ReadUserDto>(user);
                
                
                var loginResponse = new LoginResponseDto
                {
                    Token = jwtToken,
                    RefreshToken = refreshToken.Token,
                    User = userDto
                };

                return Result<LoginResponseDto>.SuccessResult(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred during login.", ex);
                return Result<LoginResponseDto>.ErrorResult("An error occurred during login.");
            }
        }

        public async Task<Result> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInfo($"Starting user registration for email: {registerDto?.Email}");
            
            if (registerDto == null)
            {
                _logger.LogError("RegisterDto is null.", new ArgumentNullException(nameof(registerDto)));
                return Result.ErrorResult("Invalid registration data.");
            }

            _logger.LogInfo($"Validating registration data for: {registerDto.Email}, Role: {registerDto.Role}");

            var validationResult = await _registerDtoValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                var validationErrors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning($"Registration validation failed: {validationErrors}");
                return Result.ErrorResult($"Validation failed: {validationErrors}");
            }

            _logger.LogInfo($"Validation passed for: {registerDto.Email}");

            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.Email,
                Role = registerDto.Role,
                BostedId = 1 // Default BostedId for all users
            };

            try
            {
                _logger.LogInfo($"Creating user in database for: {registerDto.Email}");
                var createResult = await _userManager.CreateAsync(user, registerDto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError($"User creation failed for {registerDto.Email}: {errors}", new Exception(errors));
                    return Result.ErrorResult(errors);
                }

                _logger.LogInfo($"User created successfully for: {registerDto.Email}");

                // Create role if it doesn't exist
                _logger.LogInfo($"Checking if role exists: {registerDto.Role}");
                if (!await _roleManager.RoleExistsAsync(registerDto.Role))
                {
                    _logger.LogInfo($"Creating role: {registerDto.Role}");
                    await _roleManager.CreateAsync(new IdentityRole(registerDto.Role));
                    _logger.LogInfo($"Created role: {registerDto.Role}");
                }
                else
                {
                    _logger.LogInfo($"Role already exists: {registerDto.Role}");
                }

                _logger.LogInfo($"Assigning role {registerDto.Role} to user {registerDto.Email}");
                var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError($"Failed to assign role {registerDto.Role} to user {registerDto.Email}: {errors}", new Exception(errors));
                    return Result.ErrorResult(errors);
                }

                _logger.LogInfo($"Role assigned successfully. User {registerDto.Email} registered successfully.");
                return Result.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during registration for {registerDto.Email}. Exception: {ex.Message}", ex);
                return Result.ErrorResult($"An error occurred during registration: {ex.Message}");
            }
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);
            if (user == null)
            {
                return Result.ErrorResult("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return Result.ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Result.SuccessResult();
        }

        public async Task<Result> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return Result.SuccessResult();
        }

        public async Task<Result<string>> RefreshTokenAsync(string token)
        {
            var storedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked)
            {
                return Result<string>.ErrorResult("Invalid or expired refresh token.");
            }

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return Result<string>.ErrorResult("Refresh token has expired.");
            }

            storedToken.IsUsed = true;
            _dbContext.RefreshTokens.Update(storedToken);
            await _dbContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return Result<string>.ErrorResult("User not found.");
            }

            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(user.Id);
            await SaveRefreshTokenAsync(newRefreshToken);

            return Result<string>.SuccessResult(newJwtToken);
        }

        private RefreshToken GenerateRefreshToken(string userId)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };
        }
    
        private async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("nameid", user.Id),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("bostedId", user.BostedId.ToString())
            };

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                _logger.LogError("Jwt:Key is null or empty!", new ArgumentNullException("Jwt:Key"));
            }
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}