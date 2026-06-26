using Microsoft.AspNetCore.Identity;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;

namespace SchoolMgmt.Data.Seed;

public static class DatabaseSeeder
{
    public const string PrincipalRole = "Principal";
    public const string TeacherRole = "Teacher";
    public const string StudentRole = "Student";
    public const string ParentRole = "Parent";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedPrincipalAsync(userManager);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        {
            PrincipalRole,
            TeacherRole,
            StudentRole,
            ParentRole
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                Console.WriteLine($"[Seeder] Role created: {role}");
            }
        }
    }

    private static async Task SeedPrincipalAsync(
        UserManager<ApplicationUser> userManager)
    {
        const string email = "principal@school.com";
        const string password = "Principal@123456";

        // Do not seed if already exists
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            Console.WriteLine("[Seeder] Principal already exists — skipping.");
            return;
        }

        var principal = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = "School Principal",
            Email = email,
            UserName = email,
            RoleType = RoleType.Principal,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(principal, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(principal, PrincipalRole);
            Console.WriteLine($"[Seeder] Principal created — Email: {email}");
            Console.WriteLine($"[Seeder] Password: {password}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"[Seeder] ERROR: {errors}");
        }
    }
}