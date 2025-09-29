namespace TaskMicroService.Entities;

public class ActivityEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string ResponsibleStaff { get; set; } = string.Empty; // JSON array of GUIDs
    public int ActivityType { get; set; } // Enum as int
    public string Location { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 20;
    public bool RequiresAssistance { get; set; } = false;
    public bool IsWheelchairAccessible { get; set; } = true;
    public string? SpecialRequirements { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string Participants { get; set; } = string.Empty; // JSON array of GUIDs
}
