namespace TaskMicroService.dto;

public class CreateTaskDto
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string? ImageBase64 { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public int? UserId { get; set; }

    public bool RequiresQrCodeScan { get; set; } 
}