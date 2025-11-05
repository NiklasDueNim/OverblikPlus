namespace OverblikPlus.Constants;

/// <summary>
/// Konstant klasser for roller i systemet
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string User = "User";
    public const string Relative = "Relative";
    
    /// <summary>
    /// Tjekker om en rolle er admin eller staff
    /// </summary>
    public static bool IsAdminOrStaff(string? role)
    {
        return role == Admin || role == Staff;
    }
    
    /// <summary>
    /// Tjekker om en rolle er admin
    /// </summary>
    public static bool IsAdmin(string? role)
    {
        return role == Admin;
    }
}

