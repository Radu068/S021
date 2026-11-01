using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using HotelManager.Data;
using HotelManager.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Services;

public static class HotelClock
{
    private static readonly TimeZoneInfo Zone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest");

    public static DateTime Today =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            Zone).Date;

    public static DateTime LocalTime(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            Zone);
}

public sealed class CurrentUser(IHttpContextAccessor accessor)
{
    private ClaimsPrincipal Principal =>
        accessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException();

    public int Id =>
        int.TryParse(
            Principal.FindFirstValue(ClaimTypes.NameIdentifier),
            out int id)
                ? id
                : throw new UnauthorizedAccessException();

    public bool IsReceptionist =>
        Principal.IsInRole(nameof(Receptionist));
}

public static class InputValidation
{
    public static string? Error(object input)
    {
        var errors = new List<ValidationResult>();

        return Validator.TryValidateObject(
            input,
            new ValidationContext(input),
            errors,
            true)
                ? null
                : errors.FirstOrDefault()?.ErrorMessage
                    ?? "Date invalide.";
    }
}

public sealed class SqliteWriteScope(AppDbContext db)
{
    public async Task<OperationResult> RunAsync(
        Func<Task<OperationResult>> action)
    {
        try
        {
            await db.Database.OpenConnectionAsync();

            var connection =
                (SqliteConnection)db.Database.GetDbConnection();

            await using var transaction =
                connection.BeginTransaction(
                    IsolationLevel.Serializable,
                    deferred: false);

            await using var enlistment =
                await db.Database.UseTransactionAsync(transaction);

            var result = await action();

            if (result.Success)
            {
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else
            {
                db.ChangeTracker.Clear();
            }

            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();

            return OperationResult.Fail(
                "Datele s-au schimbat. Reîncarcă pagina.");
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqliteException
            {
                SqliteErrorCode: 19
            })
        {
            db.ChangeTracker.Clear();

            return OperationResult.Fail(
                "Operația ar încălca unicitatea datelor "
                + "sau legăturile dintre înregistrări.");
        }
        catch (Exception ex) when (
            ex is SqliteException
            {
                SqliteErrorCode: 5 or 6
            }
            || ex is DbUpdateException
            {
                InnerException: SqliteException
                {
                    SqliteErrorCode: 5 or 6
                }
            })
        {
            db.ChangeTracker.Clear();

            return OperationResult.Fail(
                "Baza de date este ocupată. Reîncearcă operația.");
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}