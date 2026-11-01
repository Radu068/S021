using HotelManager.Models;

namespace HotelManager.Services.Interfaces;

public interface IReservationService
{
    Task<List<Reservation>> ListAsync();
    Task<Reservation?> FindAsync(int id);
    Task<List<Room>> RoomsAsync();
    Task<List<Guest>> GuestsAsync();

    Task<OperationResult> CreateAsync(ReservationInput input);

    Task<OperationResult> UpdateAsync(
        int id,
        ReservationInput input);

    Task<OperationResult> CancelAsync(int id, string version);
    Task<OperationResult> CheckInAsync(int id, string version);
    Task<OperationResult> CheckOutAsync(int id, string version);
}