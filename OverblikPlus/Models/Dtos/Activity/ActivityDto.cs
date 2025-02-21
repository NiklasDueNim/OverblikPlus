namespace OverblikPlus.Models.Dtos.Activity;

public class ActivityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public List<Guid> Participants { get; set; } = new();
    // Evt. "ResponsibleStaff" eller "Location" ...
}