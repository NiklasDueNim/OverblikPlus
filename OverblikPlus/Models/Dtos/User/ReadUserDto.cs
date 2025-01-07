namespace OverblikPlus.Models.Dtos.User;

public class ReadUserDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }

    public string CPRNumber { get; set; }

    public string MedicationDetails { get; set; }
}
