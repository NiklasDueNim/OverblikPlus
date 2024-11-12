using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserMicroService.Entities;

namespace UserMicroService.DataAccess;

public class UserDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<UserEntity> Users { get; set; }
    
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
}