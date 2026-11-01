using System.Security.Claims;
using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Profile;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class ViewProfileModel : PageModel
{
    private readonly IAuthService _authService;

    public ViewProfileModel(IAuthService authService)
    {
        _authService = authService;
    }

    public int AccountId { get; private set; }
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public string FullName { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string RoleLabel { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        string? idClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var account = int.TryParse(idClaim, out int userId)
            ? await _authService.GetUserByIdAsync(userId)
            : null;

        if (account is null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Auth/Login");
        }

        AccountId = account.UserId;
        FirstName = account.FirstName;
        LastName = account.LastName;
        FullName = account.GetFullName();
        Email = account.Email;

        RoleLabel = account is Receptionist
            ? "Recepționer"
            : "Client";

        return Page();
    }
}