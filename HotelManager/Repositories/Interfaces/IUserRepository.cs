using HotelManager.Models;

namespace HotelManager.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int userId);
    Task<User?> FindByEmailAsync(string normalizedEmail);
    Task<bool> TryAddAsync(User user);
    Task SaveChangesAsync();

    Task<List<Guest>> ListGuestsAsync(string? search);
    Task<Guest?> FindGuestAsync(int id);
    Task<bool> HasReservationsAsync(int guestId);
    void RemoveGuest(Guest guest);
}