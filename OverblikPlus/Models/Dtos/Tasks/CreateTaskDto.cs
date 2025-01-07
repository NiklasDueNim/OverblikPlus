namespace OverblikPlus.Models.Dtos.Tasks;

public class CreateTaskDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string ImageBase64 { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }

    public int StepNumber { get; set; }

    public string? UserId { get; set; }
    
    public string RecurrenceType { get; set; }
    
    public int RecurrenceInterval { get; set; }
    
    public DateTime StartDate { get; set; }
}