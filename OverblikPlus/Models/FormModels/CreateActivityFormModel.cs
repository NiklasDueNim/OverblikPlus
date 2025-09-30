using System.ComponentModel.DataAnnotations;
using OverblikPlus.Models.Dtos.Activity;

namespace OverblikPlus.Models.FormModels;

public class CreateActivityFormModel
{
    [Required(ErrorMessage = "Titel er påkrævet")]
    [StringLength(100, ErrorMessage = "Titel må ikke være længere end 100 tegn")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivelse er påkrævet")]
    [StringLength(500, ErrorMessage = "Beskrivelse må ikke være længere end 500 tegn")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Starttidspunkt er påkrævet")]
    public DateTime StartDateTime { get; set; } = DateTime.Now.AddHours(1);

    [Required(ErrorMessage = "Sluttidspunkt er påkrævet")]
    public DateTime EndDateTime { get; set; } = DateTime.Now.AddHours(2);

    [Required(ErrorMessage = "Aktivitetstype er påkrævet")]
    public ActivityType ActivityType { get; set; } = ActivityType.SocialAktivitet;

    [StringLength(100, ErrorMessage = "Sted må ikke være længere end 100 tegn")]
    public string? Location { get; set; }

    [Range(1, 50, ErrorMessage = "Maksimalt antal deltagere skal være mellem 1 og 50")]
    public int MaxParticipants { get; set; } = 10;

    [StringLength(200, ErrorMessage = "Særlige krav må ikke være længere end 200 tegn")]
    public string? SpecialRequirements { get; set; }

    public bool RequiresAssistance { get; set; } = false;

    public List<string> ResponsibleStaff { get; set; } = new();

    public CreateActivityDto ToCreateActivityDto(Guid createdBy)
    {
        return new CreateActivityDto
        {
            Title = Title,
            Description = Description,
            StartDateTime = StartDateTime,
            EndDateTime = EndDateTime,
            ActivityType = ActivityType,
            Location = Location,
            MaxParticipants = MaxParticipants,
            SpecialRequirements = SpecialRequirements,
            RequiresAssistance = RequiresAssistance,
            ResponsibleStaff = ResponsibleStaff,
            CreatedBy = createdBy
        };
    }
}
