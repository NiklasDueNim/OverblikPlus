namespace UserMicroService.dto;

public class UpdateUserDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    public string Username { get; set; }
    
    public string Email { get; set; }
    
    public string? Medication { get; set; }
    
    public string? Goals { get; set; }
}