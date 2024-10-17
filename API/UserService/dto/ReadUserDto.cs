namespace API.Dto;

public class ReadUserDto
{
    public int Id { get; set; }
    
    public string Username { get; set; }

    public string Role { get; set; }

    public int XP { get; set; }

    public int Level { get; set; }
}