using HotelManager.Models;
using HotelManager.Services;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Reservations;

[Authorize]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class DetailsModel(IReservationService reservations) : PageModel
{
    public bool IsReceptionist =>
        User.IsInRole(nameof(Receptionist));

    public Reservation Item { get; private set; } = null!;

    public bool CanEdit =>
        Item.Status == ReservationStatus.Reserved
        && (IsReceptionist || Item.CheckIn >= HotelClock.Today);

    public bool CanCheckIn =>
        IsReceptionist
        && Item.Status == ReservationStatus.Reserved
        && Item.CheckIn <= HotelClock.Today
        && Item.CheckOut > HotelClock.Today;

    public bool CanCheckOut =>
        IsReceptionist
        && Item.Status == ReservationStatus.CheckedIn;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await reservations.FindAsync(id);

        if (item is null)
            return NotFound();

        Item = item;
        return Page();
    }

    public Task<IActionResult> OnPostCancelAsync(
        int id,
        string version) =>
        ChangeAsync(
            id,
            () => reservations.CancelAsync(id, version));

    public Task<IActionResult> OnPostCheckInAsync(
        int id,
        string version) =>
        IsReceptionist
            ? ChangeAsync(
                id,
                () => reservations.CheckInAsync(id, version))
            : Task.FromResult<IActionResult>(Forbid());

    public Task<IActionResult> OnPostCheckOutAsync(
        int id,
        string version) =>
        IsReceptionist
            ? ChangeAsync(
                id,
                () => reservations.CheckOutAsync(id, version))
            : Task.FromResult<IActionResult>(Forbid());

    private async Task<IActionResult> ChangeAsync(
        int id,
        Func<Task<OperationResult>> action)
    {
        if (await reservations.FindAsync(id) is null)
            return NotFound();

        var result = await action();

        if (result.Success)
        {
            TempData["SuccessMessage"] =
                "Rezervarea a fost actualizată.";

            return RedirectToPage(new { id });
        }

        ModelState.AddModelError(
            "",
            result.Error ?? "Operația nu a putut fi efectuată.");

        return await OnGetAsync(id);
    }
}