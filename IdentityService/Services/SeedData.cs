using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services;

/// <summary>Создаёт роли и первоначального администратора при запуске.</summary>
/// <remarks>
/// Пароль администратора берётся из конфигурации (user-secrets или переменные окружения)
/// и не хранится в исходниках. Если пароль не задан, сидирование администратора пропускается.
/// </remarks>
public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await roleManager.CreateAsync(new IdentityRole(AppRoles.User));
        await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));

        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

        var username = configuration["SeedAdmin:Username"];
        var password = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Первоначальный администратор не создан: задайте SeedAdmin:Username и SeedAdmin:Password " +
                "через user-secrets или переменные окружения (например, SeedAdmin__Username, SeedAdmin__Password).");
            return;
        }

        if (await userManager.FindByNameAsync(username) != null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = username,
            Email = configuration["SeedAdmin:Email"] ?? $"{username}@musicstore.local",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            logger.LogError("Не удалось создать администратора: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRolesAsync(admin, new[] { AppRoles.User, AppRoles.Admin });
        logger.LogInformation("Первоначальный администратор '{Username}' создан.", username);
    }
}
