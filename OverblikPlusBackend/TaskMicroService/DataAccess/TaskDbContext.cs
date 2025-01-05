using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TaskMicroService.Entities;

namespace TaskMicroService.DataAccess
{
    public class TaskDbContext : DbContext, ITaskDbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        {
        }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<TaskStep> TaskSteps { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DatabaseFacade Database => base.Database;

        
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

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
            
            modelBuilder.Entity<CalendarEvent>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<CalendarEvent>()
                .Property(c => c.Title).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<CalendarEvent>()
                .Property(c => c.StartTime).IsRequired();
            modelBuilder.Entity<CalendarEvent>()
                .Property(c => c.EndTime).IsRequired();


            base.OnModelCreating(modelBuilder);
        }
    }
}