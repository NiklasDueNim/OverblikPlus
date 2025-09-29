namespace TaskMicroService.dtos.Activity;

public class CreateActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public List<Guid> ResponsibleStaff { get; set; } = new();
    public int ActivityType { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 20;
    public bool RequiresAssistance { get; set; } = false;
    public bool IsWheelchairAccessible { get; set; } = true;
    public string? SpecialRequirements { get; set; }
    public Guid CreatedBy { get; set; }
}
