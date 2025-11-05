namespace OverblikPlus.Models.Dtos.User;

public class ApplicationUserDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public int? BostedId { get; set; }
}
