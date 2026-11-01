using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Guests;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class DeleteModel(IGuestService guests) : PageModel
{
    public Guest Item { get; private set; } = null!;

    [BindProperty]
    public string Stamp { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await guests.FindAsync(id);

        if (guest is null)
            return NotFound();

        Item = guest;
        Stamp = guest.SecurityStamp;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await guests.DeleteAsync(id, Stamp);

        if (result.Success)
        {
            TempData["SuccessMessage"] =
                "Contul oaspetelui a fost șters.";

            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(
            "",
            result.Error ?? "Operația nu a putut fi efectuată.");

        return await OnGetAsync(id);
    }
}