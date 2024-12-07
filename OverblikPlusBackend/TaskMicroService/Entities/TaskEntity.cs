namespace TaskMicroService.Entities;

public class TaskEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; } 
    public bool IsCompleted { get; set; }
    
    public string RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime NextOccurrence { get; set; } 
    public string? UserId { get; set; }
    public bool RequiresQrCodeScan { get; set; } 
    public ICollection<TaskStep>? Steps { get; set; } = new List<TaskStep>();
}