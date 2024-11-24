using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserMicroService.Entities;

namespace UserMicroService.DataAccess;

public class UserDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
}