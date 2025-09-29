namespace OverblikPlus.Models.Dtos.Tasks;

public class CreateTaskDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string? ImageBase64 { get; set; }
    
    public string RecurrenceType { get; set; }
    
    public int RecurrenceInterval { get; set; }
    
    public DateTime StartDate { get; set; }
    public string? UserId { get; set; }
    
    public bool RequiresQrCodeScan { get; set; }
    
    // Nye properties for forbedret gentagelse
    public string MonthlyType { get; set; } = "SameDay";
    public int MonthlyDay { get; set; } = 1;
    public Dictionary<string, bool> SelectedWeekDays { get; set; } = new();
    public string EndType { get; set; } = "Never";
    public int EndAfterCount { get; set; } = 1;
    public DateTime? EndDate { get; set; }
}