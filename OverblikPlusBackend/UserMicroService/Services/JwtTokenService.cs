using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.Entities;

namespace UserMicroService.Services;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerService _logger;

    public JwtTokenService(IConfiguration configuration, ILoggerService logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("nameid", user.Id),
            new Claim(ClaimTypes.Role, user.Role ?? ""),
            new Claim("bostedId", user.BostedId.ToString())
        };

        var keyString = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            _logger.LogError("Jwt:Key is null or empty!", new ArgumentNullException("Jwt:Key"));
            throw new InvalidOperationException("JWT key is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
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

