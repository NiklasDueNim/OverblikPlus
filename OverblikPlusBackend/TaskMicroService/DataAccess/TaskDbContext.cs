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
        public DbSet<ActivityEntity> Activities { get; set; }
        public DbSet<MoodEntity> Moods { get; set; }
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
            
            modelBuilder.Entity<CalendarEvent>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<CalendarEvent>()
                .Property(c => c.Title).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<CalendarEvent>()
                .Property(c => c.StartDateTime).IsRequired();
            modelBuilder.Entity<CalendarEvent>()
                .Property(c => c.EndDateTime).IsRequired();

            // ActivityEntity configuration
            modelBuilder.Entity<ActivityEntity>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.Title).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.Description).IsRequired().HasMaxLength(1000);
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.StartDateTime).IsRequired();
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.EndDateTime).IsRequired();

            // MoodEntity configuration
            modelBuilder.Entity<MoodEntity>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<MoodEntity>()
                .Property(m => m.UserId).IsRequired();
            modelBuilder.Entity<MoodEntity>()
                .Property(m => m.Date).IsRequired();
            modelBuilder.Entity<MoodEntity>()
                .Property(m => m.Rating).IsRequired();
            // Note: Service layer handles preventing multiple moods per day by checking Date.Date
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.Location).HasMaxLength(200);
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.SpecialRequirements).HasMaxLength(500);
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.ResponsibleStaff).HasMaxLength(2000); // JSON array
            modelBuilder.Entity<ActivityEntity>()
                .Property(a => a.Participants).HasMaxLength(2000); // JSON array

            base.OnModelCreating(modelBuilder);
        }
    }
}