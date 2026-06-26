namespace OverblikPlus.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string User = "User";
    public const string Relative = "Relative";
    

    public static bool IsAdminOrStaff(string? role)
    {
        return role == Admin || role == Staff;
    }
    
    public static bool IsAdmin(string? role)
    {
        return role == Admin;
    }
}

