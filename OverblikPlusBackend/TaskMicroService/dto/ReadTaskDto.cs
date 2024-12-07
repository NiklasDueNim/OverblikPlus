namespace TaskMicroService.dto;

public class ReadTaskDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    public List<ReadTaskStepDto>? Steps { get; set; } = new();
    public string? Image { get; set; }
    public bool RequiresQrCodeScan { get; set; }

    public string UserId { get; set; }
    
    public bool isCompleted { get; set; }
    
    public string RecurrenceType { get; set; }
    
    public int RecurrenceInterval { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime NextOccurrence { get; set; } 
}