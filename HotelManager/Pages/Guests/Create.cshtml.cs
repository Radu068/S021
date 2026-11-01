using HotelManager.Models.Auth;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Guests;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class CreateModel(IGuestService guests) : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await guests.CreateAsync(Input);

        if (!result.Success)
        {
            ModelState.AddModelError(
                "",
                result.Error ?? "Operația nu a putut fi efectuată.");

            return Page();
        }

        TempData["SuccessMessage"] =
            "Contul oaspetelui a fost creat.";

        return RedirectToPage("./Index");
    }
}