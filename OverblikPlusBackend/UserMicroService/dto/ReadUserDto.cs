using System.Reflection.Metadata.Ecma335;

namespace UserMicroService.dto;

public class ReadUserDto
{
    public string Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Username { get; set; } // slettes
    
    public string Email { get; set; }
    
    public string Role { get; set; }

    public string Goals { get; set; }
    
    public ReadUserDto()
    {
        Goals = string.Empty;
    }
        
}