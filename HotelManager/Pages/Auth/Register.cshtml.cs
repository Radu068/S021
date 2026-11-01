using HotelManager.Models.Auth;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelManager.Pages.Auth;

[AllowAnonymous]
[EnableRateLimiting("auth")]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Profile/ViewProfile");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Profile/ViewProfile");

        if (!ModelState.IsValid)
            return Page();

        var result = await _authService.RegisterGuestAsync(Input);

        if (!result.Success)
        {
            ModelState.AddModelError(
                "",
                result.Error ?? "Contul nu a putut fi creat.");

            return Page();
        }

        TempData["SuccessMessage"] =
            "Contul a fost creat. Te poți autentifica.";

        return RedirectToPage(
            "/Auth/Login",
            new { returnUrl = ReturnUrl });
    }
}