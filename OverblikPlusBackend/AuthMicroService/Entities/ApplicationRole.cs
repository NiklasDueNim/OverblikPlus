using Microsoft.AspNetCore.Identity;

namespace AuthMicroService.Entities;

public class ApplicationRole : IdentityUser
{
    public string? Description { get; set; }
    
}