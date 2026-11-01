using System.Globalization;
using System.Security.Claims;
using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HotelManager.Security;

public class AuthCookieEvents(IUserRepository users)
    : CookieAuthenticationEvents
{
    private const string StampClaim = "hotelmanager:security-stamp";

    public static ClaimsPrincipal CreatePrincipal(User user) => new(
        new ClaimsIdentity(new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.GetFullName()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(StampClaim, user.SecurityStamp)
        }, CookieAuthenticationDefaults.AuthenticationScheme));

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;

        var user = int.TryParse(
            principal?.FindFirstValue(ClaimTypes.NameIdentifier),
            out int id)
                ? await users.FindByIdAsync(id)
                : null;

        if (context.Properties.IsPersistent ||
            user is null ||
            principal?.FindFirstValue(StampClaim) != user.SecurityStamp ||
            principal?.FindFirstValue(ClaimTypes.Role) != user.Role)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                context.Scheme.Name);

            return;
        }

        if (principal?.Identity?.Name != user.GetFullName() ||
            principal?.FindFirstValue(ClaimTypes.Email) != user.Email)
        {
            context.ReplacePrincipal(CreatePrincipal(user));
            context.ShouldRenew = true;
        }
    }
}