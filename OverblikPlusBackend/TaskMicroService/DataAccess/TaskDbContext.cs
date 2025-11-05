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
        public DbSet<BudgetEntity> Budgets { get; set; }
        public DbSet<ShiftEntity> Shifts { get; set; }
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
            
            // BudgetEntity configuration
            modelBuilder.Entity<BudgetEntity>()
                .HasKey(b => b.Id);
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.UserId).IsRequired();
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.Date).IsRequired();
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.Activity).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.MoneyIn).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.MoneyOut).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.Voucher).HasMaxLength(500);
            modelBuilder.Entity<BudgetEntity>()
                .Property(b => b.Note).HasMaxLength(1000);
            
            // ShiftEntity configuration
            modelBuilder.Entity<ShiftEntity>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<ShiftEntity>()
                .Property(s => s.UserId).IsRequired();
            modelBuilder.Entity<ShiftEntity>()
                .Property(s => s.StartTime).IsRequired();
            modelBuilder.Entity<ShiftEntity>()
                .Property(s => s.EndTime).IsRequired();
            
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