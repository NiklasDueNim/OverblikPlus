namespace OverblikPlus.Models.Dtos.Mood;

public class MoodDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime Date { get; set; }
    public MoodRating Rating { get; set; }
    public string Note { get; set; }
    
    public enum MoodRating
    {
        Bad = 0,
        Okay = 1,
        Good = 2
    }
}