using Microsoft.EntityFrameworkCore;
using TaskMicroService.Entities;

namespace TaskMicroService.DataAccess;

public interface ITaskDbContext
{
    DbSet<TaskEntity> Tasks { get; }
    
    DbSet<TaskStep> TaskSteps { get; }
    
    DbSet<CalendarEvent> CalendarEvents { get; }
}