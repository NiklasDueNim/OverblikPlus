namespace OverblikPlus.Dtos.User;

public class CreateUserDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MedicationDetails { get; set; } 
    public string Role { get; set; }
    public string Username { get; set; }
    
    public string Password { get; set; }
}