using Microsoft.AspNetCore.Identity;

namespace UserMicroService.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
        
    public string LastName { get; set; }
        
    public DateTime DateOfBirth { get; set; }
    public string? Medication { get; set; } = string.Empty;
    public string Role { get; set; }
    public string? Goals { get; set; } = string.Empty;
    public ApplicationUser()
    {
        Medication = string.Empty;
        Goals = string.Empty;
    }
}