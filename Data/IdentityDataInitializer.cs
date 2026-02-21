using Microsoft.AspNetCore.Identity;

namespace Sehha360.Data
{
    public class IdentityDataInitializer
    {
        public static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Doctor"))
            {
                await roleManager.CreateAsync(new IdentityRole("Doctor"));
            }

            if (!await roleManager.RoleExistsAsync("Patient"))
            {
                await roleManager.CreateAsync(new IdentityRole("Patient"));
            }
        }
    }
}
