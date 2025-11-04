namespace TaskMicroService.Entities;

public class MoodEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime Date { get; set; }
    public int Rating { get; set; } // 0 = Bad, 1 = Okay, 2 = Good
    public string? Note { get; set; }
}
