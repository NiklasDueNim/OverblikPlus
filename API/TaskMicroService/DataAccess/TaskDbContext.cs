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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure TaskEntity and its relationship with UserEntity
            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.User) // A task has one user
                .WithMany()          // A user can have many tasks
                .HasForeignKey(t => t.UserId)  // Foreign key
                .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, their tasks are deleted

            base.OnModelCreating(modelBuilder);
        }
    }
}