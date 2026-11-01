using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Repositories;

public class ReservationRepository(AppDbContext db)
    : IReservationRepository
{
    private IQueryable<Reservation> Query() =>
        db.Reservations
            .Include(x => x.Guest)
            .Include(x => x.Room)
            .Include(x => x.Receptionist);

    public Task<List<Reservation>> ListAsync(int? guestId)
    {
        var query = Query().AsNoTracking();

        if (guestId.HasValue)
        {
            query = query.Where(
                x => x.GuestId == guestId.Value);
        }

        return query
            .OrderByDescending(x => x.CheckIn)
            .ThenByDescending(x => x.ReservationId)
            .ToListAsync();
    }

    public Task<Reservation?> FindAsync(int id) =>
        Query().SingleOrDefaultAsync(
            x => x.ReservationId == id);

    public Task<bool> HasOtherCheckInAsync(
        int roomId,
        int excludedId) =>
        db.Reservations.AnyAsync(x =>
            x.RoomId == roomId
            && x.ReservationId != excludedId
            && x.Status == ReservationStatus.CheckedIn);

    public void Add(Reservation reservation) =>
        db.Reservations.Add(reservation);
}