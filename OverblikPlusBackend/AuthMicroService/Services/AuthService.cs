using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthMicroService.DataAccess;
using AuthMicroService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthMicroService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AuthDbContext _dbContext;

        public AuthService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IConfiguration configuration,
                           AuthDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, false, false);
            if (!result.Succeeded) throw new UnauthorizedAccessException("Invalid login attempt.");

            var user = await _userManager.FindByNameAsync(loginDto.Email);

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);
            await SaveRefreshTokenAsync(refreshToken);

            return jwtToken;
        }

        public async Task<RegistrationResult> RegisterAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return new RegistrationResult { Success = false, Errors = result.Errors.Select(e => e.Description) };

            await _userManager.AddToRoleAsync(user, registerDto.Role);

            return new RegistrationResult { Success = true };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByNameAsync(changePasswordDto.Username);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return true;
        }

        public Task LogoutAsync()
        {
            return _signInManager.SignOutAsync();
        }

        public async Task<string> RefreshTokenAsync(string token)
        {
            var storedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            storedToken.IsUsed = true;
            _dbContext.RefreshTokens.Update(storedToken);
            await _dbContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(user.Id);
            await SaveRefreshTokenAsync(newRefreshToken);

            return newJwtToken;
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
                new Claim("nameid", user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
