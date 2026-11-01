using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> FindByIdAsync(int id) =>
        db.Users.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == id);

    public Task<User?> FindByEmailAsync(string normalizedEmail) =>
        db.Users.SingleOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail);

    public async Task<bool> TryAddAsync(User user)
    {
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqliteException
            {
                SqliteExtendedErrorCode: 2067
            })
        {
            db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();

    public Task<Guest?> FindGuestAsync(int id) =>
        db.Guests.SingleOrDefaultAsync(
            x => x.UserId == id);

    public Task<bool> HasReservationsAsync(int guestId) =>
        db.Reservations.AnyAsync(
            x => x.GuestId == guestId);

    public void RemoveGuest(Guest guest) =>
        db.Guests.Remove(guest);

    public Task<List<Guest>> ListGuestsAsync(string? search)
    {
        var query = db.Guests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            if (term.Length > 100)
                term = term[..100];

            var normalized = term.ToUpperInvariant();

            query = query.Where(x =>
                (x.FirstName + " " + x.LastName).Contains(term)
                || x.NormalizedEmail.Contains(normalized));
        }

        return query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();
    }
}