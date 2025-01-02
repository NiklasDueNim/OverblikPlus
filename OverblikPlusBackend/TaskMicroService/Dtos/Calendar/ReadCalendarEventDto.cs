namespace TaskMicroService.Dtos.Calendar;

public class ReadCalendarEventDto
{
    public int Id { get; set; }
    
    public string UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
    
    public bool IsRecurring { get; set; }
}