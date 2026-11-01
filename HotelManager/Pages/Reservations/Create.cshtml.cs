using HotelManager.Models;
using HotelManager.Services;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManager.Pages.Reservations;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class CreateModel(
    IReservationService reservations) : PageModel
{
    [BindProperty]
    public ReservationInput Input { get; set; } = new();

    public bool IsReceptionist =>
        User.IsInRole(nameof(Receptionist));

    public List<SelectListItem> RoomOptions { get; private set; }
        = new();

    public List<SelectListItem> GuestOptions { get; private set; }
        = new();

    public async Task OnGetAsync(
        int? guestId,
        int? roomId,
        DateTime? checkIn,
        DateTime? checkOut)
    {
        Input.GuestId = IsReceptionist ? guestId : null;
        Input.RoomId = roomId ?? 0;
        Input.CheckIn = (checkIn ?? HotelClock.Today).Date;
        Input.CheckOut =
            (checkOut ?? HotelClock.Today.AddDays(1)).Date;

        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var result =
                await reservations.CreateAsync(Input);

            if (result.Success)
            {
                TempData["SuccessMessage"] =
                    "Rezervarea a fost creată.";

                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(
                "",
                result.Error ?? "Rezervarea nu a putut fi creată.");
        }

        await LoadOptionsAsync();
        return Page();
    }

    private async Task LoadOptionsAsync()
    {
        var culture =
            System.Globalization.CultureInfo.GetCultureInfo("ro-RO");

        RoomOptions = (await reservations.RoomsAsync())
            .Select(x => new SelectListItem(
                x.GetLabel() + (
                    x.PricePerNight > 0
                        ? " · "
                            + x.PricePerNight.ToString("N2", culture)
                            + " lei/noapte"
                        : " · tarif necompletat"),
                x.RoomId.ToString()))
            .ToList();

        if (IsReceptionist)
        {
            GuestOptions = (await reservations.GuestsAsync())
                .Select(x => new SelectListItem(
                    $"{x.GetFullName()} · {x.Email}",
                    x.UserId.ToString()))
                .ToList();
        }
    }
}