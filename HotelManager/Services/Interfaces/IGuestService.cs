using HotelManager.Models;
using HotelManager.Models.Auth;

namespace HotelManager.Services.Interfaces;

public interface IGuestService
{
    Task<List<Guest>> ListAsync(string? search = null);
    Task<Guest?> FindAsync(int id);

    Task<OperationResult> CreateAsync(RegisterInput input);

    Task<OperationResult> UpdateAsync(
        int id,
        GuestEditInput input);

    Task<OperationResult> DeleteAsync(
        int id,
        string stamp);
}