using HotelManager.Models;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManager.Pages.Guests;

[Authorize(Policy = "ReceptionistOnly")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel(IGuestService guests) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<Guest> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await guests.ListAsync(Search);
}