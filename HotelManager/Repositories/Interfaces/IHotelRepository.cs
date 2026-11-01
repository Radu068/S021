using HotelManager.Models;

namespace HotelManager.Repositories.Interfaces;

public interface IHotelRepository
{
    Task<List<Hotel>> ListAsync();

    Task<Hotel?> FindAsync(int id);
}