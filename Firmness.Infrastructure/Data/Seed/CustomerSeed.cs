using Microsoft.AspNetCore.Identity;
using Firmness.Domain.Entities;

namespace Firmness.Infrastructure.Data.Seed;

public static class CustomerSeed
{
    public static async Task SeedCustomerAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        const string email = "cliente@firmness.com";

        if (await userManager.FindByEmailAsync(email) != null)
            return;

        var user = new ApplicationUser
        {
            UserName = "cliente",
            Email = email,
            FullName = "Cliente General",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "Cliente123$");

        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Customer");
    }
}