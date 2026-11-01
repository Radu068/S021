using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Reservations;

[Authorize]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel(IReservationService reservations) : PageModel
{
    public bool IsReceptionist =>
        User.IsInRole(nameof(Receptionist));

    public List<Reservation> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await reservations.ListAsync();
}