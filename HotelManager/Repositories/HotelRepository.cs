using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Repositories;

public class HotelRepository(AppDbContext db) : IHotelRepository
{
    public Task<List<Hotel>> ListAsync() =>
        db.Hotels
            .AsNoTracking()
            .Include(x => x.Rooms)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.HotelId)
            .ToListAsync();

    public Task<Hotel?> FindAsync(int id) =>
        db.Hotels
            .AsNoTracking()
            .Include(x => x.Rooms)
            .SingleOrDefaultAsync(x => x.HotelId == id);
}