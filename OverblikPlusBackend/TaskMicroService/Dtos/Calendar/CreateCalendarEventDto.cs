namespace TaskMicroService.Dtos.Calendar;

public class CreateCalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime? StartDateTime { get; set; }
    
    public DateTime? EndDateTime { get; set; }
    public string? UserId { get; set; } = string.Empty;
    
    public bool? IsRecurring { get; set; }
}