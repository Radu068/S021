using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Rooms;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class EditModel(IRoomService rooms) : PageModel
{
    public Room Item { get; private set; } = null!;

    [BindProperty]
    public RoomInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id) =>
        await LoadAsync(id, copyInput: true)
            ? Page()
            : NotFound();

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!await LoadAsync(id, copyInput: false))
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await rooms.UpdateAsync(id, Input);

            if (result.Success)
            {
                TempData["SuccessMessage"] =
                    "Camera a fost actualizată.";

                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(
                "",
                result.Error ?? "Camera nu a putut fi actualizată.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        string? version,
        bool confirmDelete)
    {
        ModelState.Clear();

        if (!await LoadAsync(id, copyInput: true))
            return NotFound();

        if (!confirmDelete)
        {
            ModelState.AddModelError(
                "",
                "Bifează confirmarea pentru a șterge camera.");

            return Page();
        }

        var result = await rooms.DeleteAsync(id, version);

        if (result.Success)
        {
            TempData["SuccessMessage"] =
                "Camera a fost ștearsă.";

            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(
            "",
            result.Error ?? "Camera nu a putut fi ștearsă.");

        if (!await LoadAsync(id, copyInput: true))
            return NotFound();

        return Page();
    }

    private async Task<bool> LoadAsync(
        int id,
        bool copyInput)
    {
        var room = await rooms.FindAsync(id);

        if (room is null)
            return false;

        Item = room;

        if (copyInput)
        {
            Input = new RoomInput
            {
                HotelId = room.HotelId,
                RoomNumber = room.RoomNumber,
                NumberOfBeds = room.NumberOfBeds,
                IsApartment = room.IsApartment,
                Description = room.Description,
                PricePerNight = room.PricePerNight,
                ImagePath = room.ImagePath,
                Version = room.GetVersion()
            };
        }

        return true;
    }
}