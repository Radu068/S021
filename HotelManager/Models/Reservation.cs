namespace HotelManager.Models;

public enum ReservationStatus
{
    Reserved,
    CheckedIn,
    CheckedOut,
    Cancelled
}

public static class ReservationStatusExtensions
{
    public static string GetLabel(this ReservationStatus status) =>
        status switch
        {
            ReservationStatus.Reserved => "Rezervată",
            ReservationStatus.CheckedIn => "Cazat",
            ReservationStatus.CheckedOut => "Încheiată",
            ReservationStatus.Cancelled => "Anulată",
            _ => "Necunoscută"
        };
}

public class Reservation
{
    public int ReservationId { get; private set; }
    public int GuestId { get; private set; }
    public int RoomId { get; private set; }
    public int? ReceptionistId { get; private set; }

    public DateTime CheckIn { get; private set; }
    public DateTime CheckOut { get; private set; }
    public DateTime? ActualCheckInUtc { get; private set; }
    public DateTime? ActualCheckOutUtc { get; private set; }

    public ReservationStatus Status { get; private set; }

    public string Version { get; private set; }
        = Guid.NewGuid().ToString("N");

    public Guest Guest { get; private set; } = null!;
    public Room Room { get; private set; } = null!;
    public Receptionist? Receptionist { get; private set; }

    private Reservation() { }

    public Reservation(
        int guestId,
        int roomId,
        DateTime checkIn,
        DateTime checkOut)
    {
        GuestId = guestId;
        ChangePeriod(roomId, checkIn, checkOut);
    }

    public bool IsValidPeriod() => CheckIn < CheckOut;

    public bool Overlaps(DateTime start, DateTime end) =>
        Status is ReservationStatus.Reserved
            or ReservationStatus.CheckedIn
        && CheckIn < end.Date
        && start.Date < CheckOut;

    public void Reschedule(
        int roomId,
        DateTime checkIn,
        DateTime checkOut)
    {
        Require(ReservationStatus.Reserved);
        ChangePeriod(roomId, checkIn, checkOut);
    }

    public void AssignReceptionist(int? id)
    {
        if (id.HasValue)
            ReceptionistId = id;
    }

    public void Cancel()
    {
        Require(ReservationStatus.Reserved);
        Status = ReservationStatus.Cancelled;
        Touch();
    }

    public void CheckInGuest(int receptionistId)
    {
        Require(ReservationStatus.Reserved);

        ReceptionistId = receptionistId;
        ActualCheckInUtc = DateTime.UtcNow;
        Status = ReservationStatus.CheckedIn;

        Touch();
    }

    public void CheckOutGuest(int receptionistId)
    {
        Require(ReservationStatus.CheckedIn);

        ReceptionistId = receptionistId;
        ActualCheckOutUtc = DateTime.UtcNow;
        Status = ReservationStatus.CheckedOut;

        Touch();
    }

    private void ChangePeriod(
        int roomId,
        DateTime checkIn,
        DateTime checkOut)
    {
        if (roomId <= 0 || checkIn.Date >= checkOut.Date)
        {
            throw new ArgumentException(
                "Perioada sau camera este invalidă.");
        }

        RoomId = roomId;
        CheckIn = checkIn.Date;
        CheckOut = checkOut.Date;

        Touch();
    }

    private void Require(ReservationStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException("Stare invalidă.");
    }

    private void Touch() =>
        Version = Guid.NewGuid().ToString("N");
}