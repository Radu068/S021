using HotelManager.Models;

namespace HotelManager.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<List<Reservation>> ListAsync(int? guestId);
    Task<Reservation?> FindAsync(int id);

    Task<bool> HasOtherCheckInAsync(
        int roomId,
        int excludedId);

    void Add(Reservation reservation);
}