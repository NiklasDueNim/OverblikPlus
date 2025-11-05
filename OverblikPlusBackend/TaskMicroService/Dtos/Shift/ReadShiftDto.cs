namespace TaskMicroService.Dtos.Shift;

public class ReadShiftDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
} 