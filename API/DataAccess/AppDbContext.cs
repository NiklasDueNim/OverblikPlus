using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class AppDbContext : DbContext

{
    public DbSet<Task> Tasks { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}