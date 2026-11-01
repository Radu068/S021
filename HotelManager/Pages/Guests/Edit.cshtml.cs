using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Guests;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class EditModel(IGuestService guests) : PageModel
{
    public int GuestId { get; private set; }

    [BindProperty]
    public GuestEditInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await guests.FindAsync(id);

        if (guest is null)
            return NotFound();

        GuestId = id;

        Input = new GuestEditInput
        {
            FirstName = guest.FirstName,
            LastName = guest.LastName,
            Email = guest.Email,
            SecurityStamp = guest.SecurityStamp
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (await guests.FindAsync(id) is null)
            return NotFound();

        GuestId = id;

        if (!ModelState.IsValid)
            return Page();

        var result = await guests.UpdateAsync(id, Input);

        if (!result.Success)
        {
            ModelState.AddModelError(
                "",
                result.Error ?? "Operația nu a putut fi efectuată.");

            return Page();
        }

        TempData["SuccessMessage"] =
            "Profilul a fost actualizat. "
            + "Oaspetele se va autentifica din nou.";

        return RedirectToPage("./Index");
    }
}