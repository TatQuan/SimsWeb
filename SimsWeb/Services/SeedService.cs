using Microsoft.AspNetCore.Identity;
using SimsWeb.Data;
using SimsWeb.Models;

namespace SimsWeb.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Models.Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();


            try
            {
                logger.LogInformation("Starting create database...");
                await context.Database.EnsureCreatedAsync();

                //Add roles
                logger.LogInformation("Starting seed roles...");
                await AddRoleAsync(roleManager, "Admin");
                await AddRoleAsync(roleManager, "Student");
                await AddRoleAsync(roleManager, "Faculty");

                //Add admin user
                logger.LogInformation("Seeding admin.");
                var adminEmail = "admin@example.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new Users
                    {
                        FullName = "System Administrator",
                        UserName = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin user created successfully.");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred while seeding the database: " + ex.Message);
            }  
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

    } 
}
