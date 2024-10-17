using System.Reflection.Metadata.Ecma335;

namespace API.Dto;

public class CreateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Type { get; set; }
    
}