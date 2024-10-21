namespace OverblikPlus;

public class CreateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CPRNumber { get; set; }  // Should be encrypted before sending.
    public string MedicationDetails { get; set; }  // Same as above.
    public string Role { get; set; }  // Example: "Admin", "User", "Staff".
    public string Username { get; set; }
}