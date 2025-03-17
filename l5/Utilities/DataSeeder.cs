using Microsoft.AspNetCore.Identity;
using l5.Models;

namespace l5.Utilities
{
    public class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { "Admin", "Student", "Teacher", "Librarian" };

            foreach (var role in roles)
            {
                if(await roleManager.FindByNameAsync(role) == null)
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        Console.WriteLine($"Error creating role {role}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var user = new User
                {
                    UserName = "admin",
                    Role = "Administrator"
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    user.RefreshToken = "blablablablabla";
                    user.RefreshTokenExpiry = DateTime.Now.AddSeconds(25);
                    await userManager.UpdateAsync(user);
                }
            }
        }
    }
}
