using System.ComponentModel.DataAnnotations;

namespace HotelManager.Models;

public sealed record OperationResult(
    bool Success,
    string? Error = null)
{
    public static OperationResult Ok() => new(true);

    public static OperationResult Fail(string error) =>
        new(false, error);
}

public class GuestEditInput
{
    [Required, StringLength(50), Display(Name = "Prenume")]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50), Display(Name = "Nume")]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(254), Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required, StringLength(32)]
    public string SecurityStamp { get; set; } = "";
}

public class ReservationInput : IValidatableObject
{
    [Range(1, int.MaxValue), Display(Name = "Oaspete")]
    public int? GuestId { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Selectează camera.")]
    [Display(Name = "Cameră")]
    public int RoomId { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Sosire")]
    public DateTime? CheckIn { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Plecare")]
    public DateTime? CheckOut { get; set; }

    [StringLength(32)]
    public string? Version { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext context)
    {
        if (CheckIn.HasValue
            && CheckOut.HasValue
            && CheckIn.Value.Date >= CheckOut.Value.Date)
        {
            yield return new ValidationResult(
                "Plecarea trebuie să fie după sosire.",
                new[] { nameof(CheckOut) });
        }
    }
}

public class RoomInput : IValidatableObject
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Selectează un hotel.")]
    [Display(Name = "Hotel")]
    public int HotelId { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Numărul camerei trebuie să fie pozitiv.")]
    [Display(Name = "Număr cameră")]
    public int RoomNumber { get; set; }

    [Range(
        1,
        8,
        ErrorMessage = "Camera trebuie să aibă între 1 și 8 paturi.")]
    [Display(Name = "Număr de paturi")]
    public int NumberOfBeds { get; set; } = 2;

    [Display(Name = "Este apartament")]
    public bool IsApartment { get; set; }

    [StringLength(2000)]
    [Display(Name = "Descriere")]
    public string? Description { get; set; }

    [Range(
        0.01,
        99999.99,
        ErrorMessage = "Tariful trebuie să fie între 0,01 și 99.999,99 lei.")]
    [Display(Name = "Preț pe noapte / lei")]
    public decimal PricePerNight { get; set; }

    [StringLength(240)]
    [RegularExpression(
        @"^/images/rooms/[A-Za-z0-9_-]+\.(?:jpg|jpeg|png|webp)$",
        ErrorMessage = "Exemplu de cale validă: /images/rooms/camera-101.jpg")]
    [Display(Name = "Calea fotografiei")]
    public string? ImagePath { get; set; }

    [StringLength(64)]
    public string? Version { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext context)
    {
        if (decimal.Round(PricePerNight, 2) != PricePerNight)
        {
            yield return new ValidationResult(
                "Tariful poate avea cel mult două zecimale.",
                new[] { nameof(PricePerNight) });
        }
    }
}