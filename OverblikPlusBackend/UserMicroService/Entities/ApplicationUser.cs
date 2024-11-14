using Microsoft.AspNetCore.Identity;

namespace UserMicroService.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
        
    public string LastName { get; set; }
        
    public DateTime DateOfBirth { get; set; }
    public string? Medication { get; set; }

    public string Role { get; set; }
        
    public string? Goals { get; set; }
}