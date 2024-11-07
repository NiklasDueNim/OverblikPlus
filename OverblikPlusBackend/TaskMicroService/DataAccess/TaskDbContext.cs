using Microsoft.EntityFrameworkCore;
using TaskMicroService.Entities;

namespace TaskMicroService.DataAccess
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        {
        }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<TaskStep> TaskSteps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskEntity>()
                .HasMany(t => t.Steps) 
                .WithOne(s => s.Task)
                .HasForeignKey(s => s.TaskId)  
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<TaskStep>()
                .HasOne(ts => ts.Task)
                .WithMany(t => t.Steps)
                .HasForeignKey(ts => ts.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}