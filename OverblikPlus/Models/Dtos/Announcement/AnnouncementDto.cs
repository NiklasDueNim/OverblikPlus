namespace OverblikPlus.Models.Dtos.Announcement;

public class AnnouncementDto
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Body { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; } 
    
    public bool IsImportant { get; set; }    
}