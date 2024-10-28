namespace UserMicroService.dto;

public class CreateUserDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string CPRNumber { get; set; } // Følsomt data
    
    public string MedicationDetails { get; set; } // Følsomt data
    
    public string Role { get; set; } // F.eks. "Admin", "User", etc.
    
    public string Username { get; set; } // Unikt brugernavn
}