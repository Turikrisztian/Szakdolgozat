using MealPlanner.Security;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<UserManager<IdentityUser>>();

            if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
            {
                var createRoleResult = await roleManager.CreateAsync(
                    new IdentityRole(AppRoles.Admin));

                EnsureSucceeded(
                    createRoleResult,
                    "Az Admin szerepkör létrehozása sikertelen.");
            }

            var userId = configuration["AdminSeed:UserId"];

            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new InvalidOperationException(
                    "Az AdminSeed:UserId alapján nem található felhasználó.");
            }

            if (!user.EmailConfirmed)
            {
                throw new InvalidOperationException(
                    "Az adminisztrátornak kijelölt fiók e-mail-címe nincs megerősítve.");
            }

            if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(
                    user,
                    AppRoles.Admin);

                EnsureSucceeded(
                    addToRoleResult,
                    "Az Admin szerepkör hozzárendelése sikertelen.");
            }
        }

        private static void EnsureSucceeded(
            IdentityResult result,
            string message)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}