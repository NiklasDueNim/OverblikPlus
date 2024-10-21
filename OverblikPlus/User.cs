namespace OverblikPlus;

public class User
{
   // public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CPRNumber { get; set; } // Sensitivt data, krypteres ved behov
    public string MedicationDetails { get; set; } // Sensitivt data, krypteres ved behov
    public string Username { get; set; }
    public string Role { get; set; }  // Brugerens rolle som f.eks. "Admin", "User", "Staff"
}