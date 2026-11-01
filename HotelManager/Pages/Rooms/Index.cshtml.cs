using System.ComponentModel.DataAnnotations;
using HotelManager.Models;
using HotelManager.Services;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Rooms;

[AllowAnonymous]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class IndexModel(IRoomService rooms) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime From { get; set; } = HotelClock.Today;

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime To { get; set; } =
        HotelClock.Today.AddDays(1);

    [BindProperty(SupportsGet = true)]
    [Range(1, 8, ErrorMessage = "Alege între 1 și 8 paturi.")]
    public int Beds { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public bool? Apartment { get; set; }

    [BindProperty(SupportsGet = true)]
    [Range(
        0.01,
        99999.99,
        ErrorMessage = "Prețul maxim trebuie să fie între 0,01 și 99.999,99 lei.")]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Sort { get; set; } = "price";

    [BindProperty(SupportsGet = true)]
    public bool All { get; set; }

    public bool IsReceptionist =>
        User.IsInRole(nameof(Receptionist));

    public int Nights =>
        Math.Max(0, (To.Date - From.Date).Days);

    public List<Room> Items { get; private set; } = new();

    public async Task OnGetAsync()
    {
        All = IsReceptionist && All;

        if (!ModelState.IsValid)
            return;

        From = From.Date;
        To = To.Date;

        if (From < HotelClock.Today || To <= From)
        {
            ModelState.AddModelError(
                "",
                "Alege o sosire de astăzi înainte și o plecare după sosire.");

            return;
        }

        var items = All
            ? await rooms.ListAsync()
            : await rooms.AvailableAsync(From, To);

        IEnumerable<Room> query =
            items.Where(x => x.NumberOfBeds >= Beds);

        if (!IsReceptionist || !All)
            query = query.Where(x => x.PricePerNight > 0);

        if (Apartment.HasValue)
        {
            query = query.Where(
                x => x.IsApartment == Apartment.Value);
        }

        if (MaxPrice.HasValue)
        {
            query = query.Where(
                x => x.PricePerNight <= MaxPrice.Value);
        }

        if (Sort is not ("price" or "price-desc" or "beds"))
            Sort = "price";

        Items = Sort switch
        {
            "price-desc" => query
                .OrderByDescending(x => x.PricePerNight)
                .ThenBy(x => x.RoomNumber)
                .ToList(),

            "beds" => query
                .OrderByDescending(x => x.NumberOfBeds)
                .ThenBy(x => x.PricePerNight)
                .ToList(),

            _ => query
                .OrderBy(x => x.PricePerNight)
                .ThenBy(x => x.RoomNumber)
                .ToList()
        };
    }
}