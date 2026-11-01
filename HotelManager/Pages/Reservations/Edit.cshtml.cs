using HotelManager.Models;
using HotelManager.Services;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManager.Pages.Reservations;

[Authorize]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class EditModel(IReservationService reservations) : PageModel
{
    [BindProperty]
    public ReservationInput Input { get; set; } = new();

    public int ReservationId { get; private set; }
    public string GuestName { get; private set; } = "";

    public bool IsReceptionist =>
        User.IsInRole(nameof(Receptionist));

    public List<SelectListItem> RoomOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await reservations.FindAsync(id);

        if (item is null)
            return NotFound();

        if (item.Status != ReservationStatus.Reserved
            || (!IsReceptionist && item.CheckIn < HotelClock.Today))
        {
            return RedirectToPage("./Details", new { id });
        }

        ReservationId = id;
        GuestName = item.Guest.GetFullName();

        Input = new ReservationInput
        {
            GuestId = item.GuestId,
            RoomId = item.RoomId,
            CheckIn = item.CheckIn,
            CheckOut = item.CheckOut,
            Version = item.Version
        };

        await LoadRoomsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var item = await reservations.FindAsync(id);

        if (item is null)
            return NotFound();

        ReservationId = id;
        GuestName = item.Guest.GetFullName();

        if (ModelState.IsValid)
        {
            var result = await reservations.UpdateAsync(id, Input);

            if (result.Success)
            {
                TempData["SuccessMessage"] =
                    "Rezervarea a fost actualizată.";

                return RedirectToPage("./Details", new { id });
            }

            ModelState.AddModelError(
                "",
                result.Error ?? "Rezervarea nu a putut fi actualizată.");
        }

        await LoadRoomsAsync();
        return Page();
    }

    private async Task LoadRoomsAsync()
    {
        RoomOptions = (await reservations.RoomsAsync())
            .Select(x => new SelectListItem(
                x.GetLabel(),
                x.RoomId.ToString()))
            .ToList();
    }
}