using HotelManager.Models;
using HotelManager.Models.Auth;
using HotelManager.Security;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelManager.Pages.Auth;

[AllowAnonymous, EnableRateLimiting("auth")]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class LoginModel(IAuthService authService) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true
            ? Destination(User.IsInRole(nameof(Receptionist)))
            : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Destination(User.IsInRole(nameof(Receptionist)));

        if (!ModelState.IsValid)
            return Page();

        var account = await authService.AuthenticateAsync(Input);

        if (account is null)
        {
            ModelState.AddModelError(
                "",
                "Emailul sau parola este incorectă.");

            return Page();
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthCookieEvents.CreatePrincipal(account),
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });

        return Destination(account is Receptionist);
    }

    private IActionResult Destination(bool staff)
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
            Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage(
            staff ? "/Reception/Index" : "/Reservations/Index");
    }
}