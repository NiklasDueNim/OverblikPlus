using Microsoft.EntityFrameworkCore;
using TaskMicroService.Entities;

namespace TaskMicroService.DataAccess;

public class TaskDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }

    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }
    
}