using HotelManager.Models;

namespace HotelManager.Services.Interfaces;

public interface IRoomService
{
    Task<List<Room>> ListAsync();

    Task<Room?> FindAsync(int id);

    Task<List<Hotel>> HotelsAsync();

    Task<OperationResult> CreateAsync(RoomInput input);

    Task<OperationResult> UpdateAsync(
        int id,
        RoomInput input);

    Task<OperationResult> DeleteAsync(
        int id,
        string? version);

    Task<List<Room>> AvailableAsync(
        DateTime start,
        DateTime end);
}