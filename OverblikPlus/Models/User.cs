namespace OverblikPlus.Models;

public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }
    public bool IsAdminOrStaff => IsAdmin || Role == "Staff";
    public bool IsAdmin => Role == "Admin";
}