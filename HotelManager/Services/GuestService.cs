using HotelManager.Models;
using HotelManager.Models.Auth;
using HotelManager.Repositories.Interfaces;
using HotelManager.Services.Interfaces;

namespace HotelManager.Services;

public class GuestService(
    IUserRepository users,
    IAuthService auth,
    CurrentUser actor,
    SqliteWriteScope write) : IGuestService
{
    private void RequireStaff()
    {
        if (!actor.IsReceptionist)
            throw new UnauthorizedAccessException();
    }

    public Task<List<Guest>> ListAsync(string? search = null)
    {
        RequireStaff();
        return users.ListGuestsAsync(search);
    }

    public Task<Guest?> FindAsync(int id)
    {
        RequireStaff();
        return users.FindGuestAsync(id);
    }

    public Task<OperationResult> CreateAsync(RegisterInput input)
    {
        RequireStaff();

        return write.RunAsync(async () =>
        {
            var result = await auth.RegisterGuestAsync(input);

            return result.Success
                ? OperationResult.Ok()
                : OperationResult.Fail(
                    result.Error ?? "Contul nu a putut fi creat.");
        });
    }

    public Task<OperationResult> UpdateAsync(
        int id,
        GuestEditInput input)
    {
        RequireStaff();

        input.FirstName = (input.FirstName ?? "").Trim();
        input.LastName = (input.LastName ?? "").Trim();
        input.Email = (input.Email ?? "").Trim();

        if (InputValidation.Error(input) is string error)
        {
            return Task.FromResult(
                OperationResult.Fail(error));
        }

        return write.RunAsync(async () =>
        {
            var guest = await users.FindGuestAsync(id);

            if (guest is null)
                return OperationResult.Fail("Oaspetele nu există.");

            if (guest.SecurityStamp != input.SecurityStamp)
            {
                return OperationResult.Fail(
                    "Profilul a fost modificat. Reîncarcă pagina.");
            }

            var existing = await users.FindByEmailAsync(
                input.Email.ToUpperInvariant());

            if (existing is not null && existing.UserId != id)
            {
                return OperationResult.Fail(
                    "Emailul este deja folosit.");
            }

            guest.UpdateProfile(
                input.FirstName,
                input.LastName,
                input.Email);

            return OperationResult.Ok();
        });
    }

    public Task<OperationResult> DeleteAsync(int id, string stamp)
    {
        RequireStaff();

        return write.RunAsync(async () =>
        {
            var guest = await users.FindGuestAsync(id);

            if (guest is null)
                return OperationResult.Fail("Oaspetele nu există.");

            if (guest.SecurityStamp != stamp)
            {
                return OperationResult.Fail(
                    "Profilul s-a schimbat. Reîncarcă pagina.");
            }

            if (await users.HasReservationsAsync(id))
            {
                return OperationResult.Fail(
                    "Contul are rezervări în istoric "
                    + "și nu poate fi șters.");
            }

            users.RemoveGuest(guest);

            return OperationResult.Ok();
        });
    }
}