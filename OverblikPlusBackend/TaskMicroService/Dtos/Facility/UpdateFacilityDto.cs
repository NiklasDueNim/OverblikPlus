namespace TaskMicroService.dtos.Facility;

public class UpdateFacilityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageUrl { get; set; }
    public string? ResponsibleStaffId { get; set; }
}
