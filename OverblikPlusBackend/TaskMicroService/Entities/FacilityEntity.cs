namespace TaskMicroService.Entities;

// A physical thing/place at the bosted (car, hall, gaming room, ...) with an optional
// picture and an optional responsible staff member.
public class FacilityEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ResponsibleStaffId { get; set; }
    public int? BostedId { get; set; }
    public DateTime CreatedAt { get; set; }
}
