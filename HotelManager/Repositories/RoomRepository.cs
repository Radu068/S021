using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using HotelManager.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Repositories;

public class RoomRepository(AppDbContext db) : IRoomRepository
{
    public Task<List<Room>> ListAsync() =>
        db.Rooms
            .AsNoTracking()
            .Include(x => x.Hotel)
            .OrderBy(x => x.HotelId)
            .ThenBy(x => x.RoomNumber)
            .ToListAsync();

    public Task<Room?> FindAsync(int id) =>
        db.Rooms
            .AsNoTracking()
            .Include(x => x.Hotel)
            .SingleOrDefaultAsync(x => x.RoomId == id);

    public Task<Room?> FindForUpdateAsync(int id) =>
        db.Rooms
            .Include(x => x.Hotel)
            .SingleOrDefaultAsync(x => x.RoomId == id);

    public Task<bool> NumberExistsAsync(
        int hotelId,
        int roomNumber,
        int excludedId = 0) =>
        db.Rooms.AnyAsync(x =>
            x.HotelId == hotelId
            && x.RoomNumber == roomNumber
            && x.RoomId != excludedId);

    public Task<bool> HasOpenReservationsAsync(int roomId) =>
        db.Reservations.AnyAsync(x =>
            x.RoomId == roomId
            && (x.Status == ReservationStatus.Reserved
                || x.Status == ReservationStatus.CheckedIn));

    public Task<bool> HasReservationsAsync(int roomId) =>
        db.Reservations.AnyAsync(x => x.RoomId == roomId);

    private IQueryable<Reservation> Blocking(
        DateTime start,
        DateTime end)
    {
        var today = HotelClock.Today;

        return db.Reservations.Where(x =>
            (
                x.Status == ReservationStatus.Reserved
                && x.CheckIn < end
                && start < x.CheckOut
            )
            ||
            (
                x.Status == ReservationStatus.CheckedIn
                && (
                    (x.CheckIn < end && start < x.CheckOut)
                    || x.CheckOut <= today
                )
            ));
    }

    public Task<bool> HasOverlapAsync(
        int roomId,
        DateTime start,
        DateTime end,
        int excludedId = 0) =>
        Blocking(start, end).AnyAsync(x =>
            x.RoomId == roomId
            && x.ReservationId != excludedId);

    public Task<List<Room>> AvailableAsync(
        DateTime start,
        DateTime end)
    {
        var blocked = Blocking(start, end)
            .Select(x => x.RoomId);

        return db.Rooms
            .AsNoTracking()
            .Include(x => x.Hotel)
            .Where(x => !blocked.Contains(x.RoomId))
            .OrderBy(x => x.HotelId)
            .ThenBy(x => x.RoomNumber)
            .ToListAsync();
    }

    public void Add(Room room) =>
        db.Rooms.Add(room);

    public void Remove(Room room) =>
        db.Rooms.Remove(room);
}