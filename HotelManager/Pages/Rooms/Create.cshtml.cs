using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManager.Pages.Rooms;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class CreateModel(IRoomService rooms) : PageModel
{
    [BindProperty]
    public RoomInput Input { get; set; } = new();

    public List<SelectListItem> HotelOptions { get; private set; }
        = new();

    public async Task OnGetAsync()
    {
        await LoadHotelsAsync();

        if (HotelOptions.Count > 0)
        {
            Input.HotelId = int.Parse(
                HotelOptions[0].Value);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var result = await rooms.CreateAsync(Input);

            if (result.Success)
            {
                TempData["SuccessMessage"] =
                    "Camera a fost adăugată.";

                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(
                "",
                result.Error ?? "Camera nu a putut fi adăugată.");
        }

        await LoadHotelsAsync();

        return Page();
    }

    private async Task LoadHotelsAsync()
    {
        HotelOptions = (await rooms.HotelsAsync())
            .Select(x => new SelectListItem(
                x.Name,
                x.HotelId.ToString()))
            .ToList();
    }
}