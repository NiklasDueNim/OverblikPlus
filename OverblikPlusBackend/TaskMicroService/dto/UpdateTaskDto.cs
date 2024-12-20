namespace TaskMicroService.dto;

public class UpdateTaskDto
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string? ImageUrl { get; set; } 

    public bool IsCompleted { get; set; }
    
    public string? UserId { get; set; }

    public bool RequiresQrCodeScan { get; set; } 
    
    public string RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; }
}