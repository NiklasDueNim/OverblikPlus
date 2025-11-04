namespace TaskMicroService.Dtos.Mood;

public class ReadMoodDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime Date { get; set; }
    public int Rating { get; set; }
    public string? Note { get; set; }
}
