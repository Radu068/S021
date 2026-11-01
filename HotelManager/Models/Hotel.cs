namespace HotelManager.Models;

public class Hotel
{
    public int HotelId { get; private set; }
    public string Name { get; private set; } = "";
    public List<Room> Rooms { get; private set; } = new();

    public int NumberOfRooms => Rooms.Count;

    public int NumberOfApartments =>
        Rooms.Count(x => x.IsApartment);

    private Hotel() { }

    public Hotel(string name)
    {
        Name = name;
    }

    public void AddRoom(Room room)
    {
        if (Rooms.Any(x => x.RoomNumber == room.RoomNumber))
        {
            throw new InvalidOperationException(
                "Număr de cameră duplicat.");
        }

        Rooms.Add(room);
    }
}