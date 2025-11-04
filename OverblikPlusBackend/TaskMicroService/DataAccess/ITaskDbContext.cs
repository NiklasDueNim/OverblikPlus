using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TaskMicroService.Entities;

namespace TaskMicroService.DataAccess;

public interface ITaskDbContext
{
    DbSet<TaskEntity> Tasks { get; }
    
    DbSet<TaskStep> TaskSteps { get; }
    
    DbSet<CalendarEvent> CalendarEvents { get; }
    
    DbSet<MoodEntity> Moods { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    DatabaseFacade Database { get; }
}