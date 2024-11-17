using Microsoft.AspNetCore.Identity;

namespace AuthMicroService;

public class SeedData
{
    public static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Seeding roles...");

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Admin", "Staff", "User" };
        
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    
}