using Microsoft.EntityFrameworkCore;
using UserMicroService.Entities;

namespace UserMicroService.DataAccess;

public class UserDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
}