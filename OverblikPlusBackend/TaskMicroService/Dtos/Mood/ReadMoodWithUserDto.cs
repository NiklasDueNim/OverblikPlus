namespace TaskMicroService.Dtos.Mood;

public class ReadMoodWithUserDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string UserFirstName { get; set; }
    public string UserLastName { get; set; }
    public DateTime Date { get; set; }
    public int Rating { get; set; }
    public string? Note { get; set; }
}
