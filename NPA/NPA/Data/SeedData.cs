using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NPA.Data.Models;

namespace NPA.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NpaDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        // Seed roles
        string[] roles = [nameof(AppRole.Admin), nameof(AppRole.ProjectManager), nameof(AppRole.StandardUser)];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed default admin
        const string adminEmail = "admin@npa.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                Role = AppRole.Admin,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, nameof(AppRole.Admin));
        }

        // Seed a project manager
        const string pmEmail = "pm@npa.local";
        if (await userManager.FindByEmailAsync(pmEmail) is null)
        {
            var pm = new ApplicationUser
            {
                UserName = pmEmail,
                Email = pmEmail,
                FullName = "Project Manager",
                Role = AppRole.ProjectManager,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(pm, "Pm@12345!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(pm, nameof(AppRole.ProjectManager));
        }

        // Seed a standard user
        const string userEmail = "user@npa.local";
        if (await userManager.FindByEmailAsync(userEmail) is null)
        {
            var user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = "Standard User",
                Role = AppRole.StandardUser,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "User@1234!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, nameof(AppRole.StandardUser));
        }
    }
}
