namespace OverblikPlus.Models.Dtos.Shift;

public class ShiftDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}