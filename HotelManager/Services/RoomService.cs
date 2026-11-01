using System.ComponentModel.DataAnnotations;
using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using HotelManager.Services.Interfaces;

namespace HotelManager.Services;

public class RoomService(
    IRoomRepository rooms,
    IHotelRepository hotels,
    CurrentUser actor,
    SqliteWriteScope write) : IRoomService
{
    private void RequireStaff()
    {
        if (!actor.IsReceptionist)
            throw new UnauthorizedAccessException();
    }

    public Task<List<Room>> ListAsync()
    {
        RequireStaff();
        return rooms.ListAsync();
    }

    public Task<Room?> FindAsync(int id)
    {
        RequireStaff();
        return rooms.FindAsync(id);
    }

    public Task<List<Hotel>> HotelsAsync()
    {
        RequireStaff();
        return hotels.ListAsync();
    }

    public Task<OperationResult> CreateAsync(RoomInput input)
    {
        RequireStaff();

        input.Description = (input.Description ?? "").Trim();
        input.ImagePath = (input.ImagePath ?? "").Trim();

        if (InputValidation.Error(input) is string error)
        {
            return Task.FromResult(
                OperationResult.Fail(error));
        }

        return write.RunAsync(async () =>
        {
            if (await hotels.FindAsync(input.HotelId) is null)
            {
                return OperationResult.Fail(
                    "Hotelul selectat nu există.");
            }

            if (await rooms.NumberExistsAsync(
                input.HotelId,
                input.RoomNumber))
            {
                return OperationResult.Fail(
                    "Există deja o cameră cu acest număr în hotel.");
            }

            var room = new Room(
                input.HotelId,
                input.RoomNumber,
                input.NumberOfBeds,
                input.IsApartment);

            room.UpdatePresentation(
                input.Description,
                input.PricePerNight,
                input.ImagePath);

            rooms.Add(room);

            return OperationResult.Ok();
        });
    }

    public Task<OperationResult> UpdateAsync(
        int id,
        RoomInput input)
    {
        RequireStaff();

        input.Description = (input.Description ?? "").Trim();
        input.ImagePath = (input.ImagePath ?? "").Trim();

        if (InputValidation.Error(input) is string error)
        {
            return Task.FromResult(
                OperationResult.Fail(error));
        }

        return write.RunAsync(async () =>
        {
            var room = await rooms.FindForUpdateAsync(id);

            if (room is null)
                return OperationResult.Fail("Camera nu există.");

            if (room.GetVersion() != input.Version)
            {
                return OperationResult.Fail(
                    "Datele camerei s-au schimbat. Reîncarcă pagina.");
            }

            if (room.HotelId != input.HotelId)
            {
                return OperationResult.Fail(
                    "Hotelul camerei nu poate fi schimbat.");
            }

            bool changed =
                room.RoomNumber != input.RoomNumber
                || room.NumberOfBeds != input.NumberOfBeds
                || room.IsApartment != input.IsApartment
                || room.Description != input.Description
                || room.PricePerNight != input.PricePerNight
                || room.ImagePath != input.ImagePath;

            if (!changed)
                return OperationResult.Ok();

            if (await rooms.HasOpenReservationsAsync(id))
            {
                return OperationResult.Fail(
                    "Camera are rezervări neîncheiate. "
                    + "Încheie sau anulează aceste rezervări înainte de modificare.");
            }

            if (await rooms.NumberExistsAsync(
                room.HotelId,
                input.RoomNumber,
                id))
            {
                return OperationResult.Fail(
                    "Există deja o cameră cu acest număr în hotel.");
            }

            room.UpdateDetails(
                input.RoomNumber,
                input.NumberOfBeds,
                input.IsApartment);

            room.UpdatePresentation(
                input.Description,
                input.PricePerNight,
                input.ImagePath);

            return OperationResult.Ok();
        });
    }

    public Task<OperationResult> DeleteAsync(
        int id,
        string? version)
    {
        RequireStaff();

        return write.RunAsync(async () =>
        {
            var room = await rooms.FindForUpdateAsync(id);

            if (room is null)
                return OperationResult.Fail("Camera nu există.");

            if (room.GetVersion() != version)
            {
                return OperationResult.Fail(
                    "Datele camerei s-au schimbat. Reîncarcă pagina.");
            }

            if (await rooms.HasReservationsAsync(id))
            {
                return OperationResult.Fail(
                    "Camera are rezervări în istoric și nu poate fi ștearsă.");
            }

            rooms.Remove(room);

            return OperationResult.Ok();
        });
    }

    public Task<List<Room>> AvailableAsync(
        DateTime start,
        DateTime end)
    {
        start = start.Date;
        end = end.Date;

        if (start < HotelClock.Today || start >= end)
        {
            throw new ValidationException(
                "Alege o perioadă validă, începând de astăzi.");
        }

        return rooms.AvailableAsync(start, end);
    }
}