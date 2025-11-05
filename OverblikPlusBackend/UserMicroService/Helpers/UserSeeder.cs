using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.Entities;

namespace UserMicroService.Helpers;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider, ILoggerService logger)
    {
        try
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Definer test-brugere med forskellige roller
            var testUsers = new[]
            {
                new
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@overblikplus.dk",
                    Password = "Admin123!",
                    Role = "Admin",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1980, 1, 1)
                },
                new
                {
                    FirstName = "Lars",
                    LastName = "Hansen",
                    Email = "staff1@overblikplus.dk",
                    Password = "Staff123!",
                    Role = "Staff",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1985, 5, 15)
                },
                new
                {
                    FirstName = "Mette",
                    LastName = "Nielsen",
                    Email = "staff2@overblikplus.dk",
                    Password = "Staff123!",
                    Role = "Staff",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1990, 8, 22)
                },
                new
                {
                    FirstName = "Erik",
                    LastName = "Andersen",
                    Email = "beboer1@overblikplus.dk",
                    Password = "Beboer123!",
                    Role = "User",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1955, 3, 10)
                },
                new
                {
                    FirstName = "Grethe",
                    LastName = "Petersen",
                    Email = "beboer2@overblikplus.dk",
                    Password = "Beboer123!",
                    Role = "User",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1960, 7, 18)
                },
                new
                {
                    FirstName = "Ole",
                    LastName = "Jensen",
                    Email = "beboer3@overblikplus.dk",
                    Password = "Beboer123!",
                    Role = "User",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1948, 12, 5)
                },
                new
                {
                    FirstName = "Kirsten",
                    LastName = "Møller",
                    Email = "relative1@overblikplus.dk",
                    Password = "Relative123!",
                    Role = "Relative",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1975, 4, 30)
                },
                new
                {
                    FirstName = "Henrik",
                    LastName = "Larsen",
                    Email = "relative2@overblikplus.dk",
                    Password = "Relative123!",
                    Role = "Relative",
                    BostedId = 1,
                    DateOfBirth = new DateTime(1978, 9, 12)
                }
            };

            // Opret roller hvis de ikke eksisterer
            var roles = new[] { "Admin", "Staff", "User", "Relative" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInfo($"Role '{roleName}' created.");
                }
            }

            // Opret brugere
            foreach (var userData in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);
                if (existingUser != null)
                {
                    logger.LogInfo($"User '{userData.Email}' already exists, skipping.");
                    continue;
                }

                var user = new ApplicationUser
                {
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    Email = userData.Email,
                    UserName = userData.Email,
                    Role = userData.Role,
                    BostedId = userData.BostedId,
                    DateOfBirth = userData.DateOfBirth,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                    logger.LogInfo($"User '{userData.Email}' ({userData.Role}) created successfully with password: {userData.Password}");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError($"Failed to create user '{userData.Email}': {errors}", new Exception(errors));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error seeding users: {ex.Message}", ex);
        }
    }
}

