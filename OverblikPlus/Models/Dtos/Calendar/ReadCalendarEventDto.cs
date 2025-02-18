namespace OverblikPlus.Models.Dtos.Calendar;

public class ReadCalendarEventDto
{
    public int Id { get; set; }
    
    public string UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartDateTime { get; set; }
    
    public DateTime EndDateTime { get; set; }
    
    public bool IsRecurring { get; set; }
}