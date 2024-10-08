using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class AppDbContext : DbContext

{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}