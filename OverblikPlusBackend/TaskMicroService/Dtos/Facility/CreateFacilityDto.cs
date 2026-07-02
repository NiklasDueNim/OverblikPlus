namespace TaskMicroService.dtos.Facility;

public class CreateFacilityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ResponsibleStaffId { get; set; }
    public int? BostedId { get; set; }
}
