using Microsoft.EntityFrameworkCore;
using TaskMicroService.Entities;

namespace TaskMicroService.Services.Interfaces;

public class ITaskDbContext
{
    DbSet<TaskEntity> Tasks { get; }
    
    DbSet<TaskStep> TaskSteps { get; }
    
    DbSet<CalendarEvent> CalendarEvents { get; }

}