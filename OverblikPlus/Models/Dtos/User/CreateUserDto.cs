using System.ComponentModel.DataAnnotations;

namespace OverblikPlus.Models.Dtos.User;

public class CreateUserDto
{
    [Required(ErrorMessage = "Fornavn er påkrævet.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Efternavn er påkrævet.")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Email er påkrævet.")]
    [EmailAddress(ErrorMessage = "Ugyldig email.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Adgangskode er påkrævet.")]
    public string Password { get; set; }
    
    public string Role { get; set; }
}