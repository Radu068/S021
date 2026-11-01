using System.ComponentModel.DataAnnotations;
using HotelManager.Models;
using HotelManager.Services;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Reception;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class IndexModel(
    IReservationService reservations,
    IRoomService rooms) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime From { get; set; } = HotelClock.Today;

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime To { get; set; } = HotelClock.Today.AddDays(1);

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

    public List<Reservation> Arrivals { get; private set; } = new();
    public List<Reservation> Stays { get; private set; } = new();
    public List<Room> Available { get; private set; } = new();

    public int DueDepartures =>
        Stays.Count(x => x.CheckOut <= HotelClock.Today);

    public int Nights => Math.Max(0, (To.Date - From.Date).Days);

    public async Task OnGetAsync()
    {
        var today = HotelClock.Today;
        var items = await reservations.ListAsync();

        Arrivals = items
            .Where(x =>
                x.Status == ReservationStatus.Reserved
                && x.CheckIn <= today
                && x.CheckOut > today)
            .OrderBy(x => x.CheckIn)
            .ThenBy(x => x.ReservationId)
            .ToList();

        Stays = items
            .Where(x => x.Status == ReservationStatus.CheckedIn)
            .OrderBy(x => x.CheckOut)
            .ThenBy(x => x.ReservationId)
            .ToList();

        if (!ModelState.IsValid)
            return;

        From = From.Date;
        To = To.Date;

        if (From < today || To <= From)
        {
            ModelState.AddModelError(
                "",
                "Alege o sosire de astăzi înainte și o plecare după sosire.");

            return;
        }

        if (Sort is not ("price" or "price-desc" or "beds"))
            Sort = "price";

        try
        {
            var available = await rooms.AvailableAsync(From, To);

            IEnumerable<Room> query = available
                .Where(x => x.NumberOfBeds >= Beds);

            if (Apartment.HasValue)
            {
                query = query.Where(
                    x => x.IsApartment == Apartment.Value);
            }

            if (MaxPrice.HasValue)
            {
                query = query.Where(x =>
                    x.PricePerNight > 0
                    && x.PricePerNight <= MaxPrice.Value);
            }

            // Camerele fără tarif apar la sfârșitul listei.
            var ordered = query.OrderBy(x => x.PricePerNight <= 0);

            Available = Sort switch
            {
                "price-desc" => ordered
                    .ThenByDescending(x => x.PricePerNight)
                    .ThenBy(x => x.RoomNumber)
                    .ToList(),

                "beds" => ordered
                    .ThenByDescending(x => x.NumberOfBeds)
                    .ThenBy(x => x.PricePerNight)
                    .ThenBy(x => x.RoomNumber)
                    .ToList(),

                _ => ordered
                    .ThenBy(x => x.PricePerNight)
                    .ThenBy(x => x.RoomNumber)
                    .ToList()
            };
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
    }
}