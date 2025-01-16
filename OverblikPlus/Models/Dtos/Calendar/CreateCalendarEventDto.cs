namespace OverblikPlus.Models.Dtos.Calendar;

public class CreateCalendarEventDto
{
    public string? Title { get; set; } = string.Empty;
    
    public string? Description { get; set; } = string.Empty;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
    
    public string? UserId { get; set; } = string.Empty;
    
    public bool? IsRecurring { get; set; }
}