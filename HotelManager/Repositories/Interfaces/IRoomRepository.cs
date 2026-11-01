using HotelManager.Models;

namespace HotelManager.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<List<Room>> ListAsync();

    Task<Room?> FindAsync(int id);

    Task<Room?> FindForUpdateAsync(int id);

    Task<bool> NumberExistsAsync(
        int hotelId,
        int roomNumber,
        int excludedId = 0);

    Task<bool> HasOpenReservationsAsync(int roomId);

    Task<bool> HasReservationsAsync(int roomId);

    Task<bool> HasOverlapAsync(
        int roomId,
        DateTime start,
        DateTime end,
        int excludedId = 0);

    Task<List<Room>> AvailableAsync(
        DateTime start,
        DateTime end);

    void Add(Room room);

    void Remove(Room room);
}