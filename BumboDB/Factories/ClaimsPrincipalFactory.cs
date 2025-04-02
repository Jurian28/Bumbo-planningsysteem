using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Reflection;
using BumboDB.Models;

namespace BumboDB.Factories;
public class ClaimsPrincipalFactory(
    UserManager<Employee> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor
) : UserClaimsPrincipalFactory<Employee, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Employee user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var excludedProperties = new HashSet<string>
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "PhoneNumber",
            "PhoneNumberConfirmed",
            "TwoFactorEnabled",
            "LockoutEnd",
            "LockoutEnabled",
            "AccessFailedCount"
        };

        var customProperties = typeof(Employee).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in customProperties)
        {
            if (property.Name != nameof(IdentityUser.Id) && property.CanRead && !excludedProperties.Contains(property.Name))
            {
                var value = property.GetValue(user)?.ToString();
                if (value != null)
                {
                    identity.AddClaim(new Claim(property.Name, value));
                }
            }
        }

        return identity;
    }
}
