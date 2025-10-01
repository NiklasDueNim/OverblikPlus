namespace OverblikPlus.Models.Dtos.Activity;

public class ActivityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public List<string> Participants { get; set; } = new();
    public List<string> ResponsibleStaff { get; set; } = new();
    public ActivityType ActivityType { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 20;
    public bool RequiresAssistance { get; set; } = false;
    public bool IsWheelchairAccessible { get; set; } = true;
    public string? SpecialRequirements { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

public enum ActivityType
{
    FysiskAktivitet,    // Motion, gåture
    SocialAktivitet,    // Kaffe, spil
    KreativAktivitet,   // Håndarbejde, maleri
    KognitivAktivitet,  // Læsning, puslespil
    MusikAktivitet,     // Sang, musik
    MadAktivitet,       // Kogning, bagning
    Udflugt,           // Ture udenfor institutionen
    Andet              // Andre aktiviteter
}