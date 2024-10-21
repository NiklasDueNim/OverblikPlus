namespace OverblikPlus;

public class ReadUserDto
{
    public int Id { get; set; }
        
    public string FirstName { get; set; }
        
    public string LastName { get; set; }
        
    public string Username { get; set; }
        
    public string Role { get; set; }  // F.eks. "Admin", "User", "Staff"

    // Følsomme oplysninger, der muligvis kræver tilladelser
    public string CPRNumber { get; set; }

    public string MedicationDetails { get; set; }
}