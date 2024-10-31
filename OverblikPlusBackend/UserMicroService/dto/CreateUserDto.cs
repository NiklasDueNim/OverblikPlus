namespace UserMicroService.dto;

public class CreateUserDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string CPRNumber { get; set; }
    
    public string MedicationDetails { get; set; }
    
    public string Role { get; set; }
    
    public string Username { get; set; }
}