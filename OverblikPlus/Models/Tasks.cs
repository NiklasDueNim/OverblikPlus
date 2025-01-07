using OverblikPlus.Models.Dtos.TaskSteps;

namespace OverblikPlus.Models;

public class Tasks
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string? Image { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }
    
    public List<TaskStep> Steps { get; set; } = new List<TaskStep>();

    public string? UserId { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public string RecurrenceType { get; set; }
    
    public int RecurrenceInterval { get; set; }
    public DateTime? NextOccurrence { get; set; }

    public DateTime StartDate { get; set; }
    public bool HasSteps { get; set; }
}