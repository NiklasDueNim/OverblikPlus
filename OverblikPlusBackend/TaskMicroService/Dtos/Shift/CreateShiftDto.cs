namespace TaskMicroService.Dtos.Shift;

public class CreateShiftDto
{
    public string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
