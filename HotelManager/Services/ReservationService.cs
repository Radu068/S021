using HotelManager.Models;
using HotelManager.Repositories.Interfaces;
using HotelManager.Services.Interfaces;

namespace HotelManager.Services;

public class ReservationService(
    IReservationRepository reservations,
    IRoomRepository rooms,
    IUserRepository users,
    CurrentUser actor,
    SqliteWriteScope write) : IReservationService
{
    private bool CanAccess(Reservation reservation) =>
        actor.IsReceptionist || reservation.GuestId == actor.Id;

    public Task<List<Reservation>> ListAsync() =>
        reservations.ListAsync(
            actor.IsReceptionist ? null : actor.Id);

    public async Task<Reservation?> FindAsync(int id)
    {
        var item = await reservations.FindAsync(id);

        return item is not null && CanAccess(item)
            ? item
            : null;
    }

    public async Task<List<Room>> RoomsAsync()
    {
        var items = await rooms.ListAsync();

        return actor.IsReceptionist
            ? items
            : items.Where(x => x.PricePerNight > 0).ToList();
    }

    public Task<List<Guest>> GuestsAsync() =>
        actor.IsReceptionist
            ? users.ListGuestsAsync(null)
            : Task.FromResult(new List<Guest>());

    private static string? Validate(ReservationInput input)
    {
        var error = InputValidation.Error(input);

        if (error is not null)
            return error;

        if (input.CheckIn!.Value.Date < HotelClock.Today)
            return "Sosirea nu poate fi în trecut.";

        return null;
    }

    public Task<OperationResult> CreateAsync(
        ReservationInput input)
    {
        if (Validate(input) is string error)
        {
            return Task.FromResult(
                OperationResult.Fail(error));
        }

        return write.RunAsync(async () =>
        {
            int? guestId = actor.IsReceptionist
                ? input.GuestId
                : actor.Id;

            if (!guestId.HasValue ||
                await users.FindGuestAsync(guestId.Value) is null)
            {
                return OperationResult.Fail(
                    "Selectează un oaspete valid.");
            }

            var selectedRoom =
                await rooms.FindAsync(input.RoomId);

            if (selectedRoom is null)
                return OperationResult.Fail("Camera nu există.");

            if (!actor.IsReceptionist &&
                selectedRoom.PricePerNight <= 0)
            {
                return OperationResult.Fail(
                    "Camera nu este disponibilă pentru rezervare online.");
            }

            var start = input.CheckIn!.Value.Date;
            var end = input.CheckOut!.Value.Date;

            if (await rooms.HasOverlapAsync(
                input.RoomId,
                start,
                end))
            {
                return OperationResult.Fail(
                    "Camera este ocupată în perioada aleasă.");
            }

            var item = new Reservation(
                guestId.Value,
                input.RoomId,
                start,
                end);

            item.AssignReceptionist(
                actor.IsReceptionist ? actor.Id : null);

            reservations.Add(item);

            return OperationResult.Ok();
        });
    }

    public Task<OperationResult> UpdateAsync(
        int id,
        ReservationInput input)
    {
        if (Validate(input) is string error)
        {
            return Task.FromResult(
                OperationResult.Fail(error));
        }

        return write.RunAsync(async () =>
        {
            var item = await FindAsync(id);

            var error = EditableError(
                item,
                input.Version);

            if (error is not null)
                return OperationResult.Fail(error);

            var selectedRoom =
                await rooms.FindAsync(input.RoomId);

            if (selectedRoom is null)
                return OperationResult.Fail("Camera nu există.");

            if (!actor.IsReceptionist &&
                selectedRoom.PricePerNight <= 0)
            {
                return OperationResult.Fail(
                    "Camera nu este disponibilă pentru rezervare online.");
            }

            var start = input.CheckIn!.Value.Date;
            var end = input.CheckOut!.Value.Date;

            if (await rooms.HasOverlapAsync(
                input.RoomId,
                start,
                end,
                id))
            {
                return OperationResult.Fail(
                    "Camera este ocupată în perioada aleasă.");
            }

            item!.Reschedule(
                input.RoomId,
                start,
                end);

            item.AssignReceptionist(
                actor.IsReceptionist ? actor.Id : null);

            return OperationResult.Ok();
        });
    }

    private string? EditableError(
        Reservation? item,
        string? version)
    {
        if (item is null)
        {
            return "Rezervarea nu există sau nu îți aparține.";
        }

        if (item.Version != version)
        {
            return "Rezervarea s-a schimbat. Reîncarcă pagina.";
        }

        if (item.Status != ReservationStatus.Reserved)
        {
            return "Doar rezervările neîncepute pot fi "
                + "modificate sau anulate.";
        }

        if (!actor.IsReceptionist &&
            item.CheckIn < HotelClock.Today)
        {
            return "Pentru această rezervare trebuie "
                + "să contactezi recepția.";
        }

        return null;
    }

    public Task<OperationResult> CancelAsync(
        int id,
        string version) =>
        write.RunAsync(async () =>
        {
            var item = await FindAsync(id);
            var error = EditableError(item, version);

            if (error is not null)
                return OperationResult.Fail(error);

            item!.Cancel();

            item.AssignReceptionist(
                actor.IsReceptionist ? actor.Id : null);

            return OperationResult.Ok();
        });

    public Task<OperationResult> CheckInAsync(
        int id,
        string version) =>
        ChangeStayAsync(id, version, checkOut: false);

    public Task<OperationResult> CheckOutAsync(
        int id,
        string version) =>
        ChangeStayAsync(id, version, checkOut: true);

    private Task<OperationResult> ChangeStayAsync(
        int id,
        string version,
        bool checkOut)
    {
        if (!actor.IsReceptionist)
        {
            return Task.FromResult(
                OperationResult.Fail(
                    "Operația este rezervată recepției."));
        }

        return write.RunAsync(async () =>
        {
            var item = await reservations.FindAsync(id);

            if (item is null)
                return OperationResult.Fail("Rezervarea nu există.");

            if (item.Version != version)
            {
                return OperationResult.Fail(
                    "Rezervarea s-a schimbat. Reîncarcă pagina.");
            }

            if (checkOut)
            {
                if (item.Status != ReservationStatus.CheckedIn)
                {
                    return OperationResult.Fail(
                        "Oaspetele nu are un check-in activ.");
                }

                item.CheckOutGuest(actor.Id);
            }
            else
            {
                if (item.Status != ReservationStatus.Reserved ||
                    item.CheckIn > HotelClock.Today ||
                    item.CheckOut <= HotelClock.Today)
                {
                    return OperationResult.Fail(
                        "Check-in-ul este permis doar "
                        + "în perioada rezervării.");
                }

                if (await reservations.HasOtherCheckInAsync(
                    item.RoomId,
                    id))
                {
                    return OperationResult.Fail(
                        "Camera are deja un oaspete cazat.");
                }

                item.CheckInGuest(actor.Id);
            }

            return OperationResult.Ok();
        });
    }
}