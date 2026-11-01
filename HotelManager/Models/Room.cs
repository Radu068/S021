using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HotelManager.Models;

public class Room
{
    public int RoomId { get; private set; }
    public int HotelId { get; private set; }
    public int RoomNumber { get; private set; }
    public int NumberOfBeds { get; private set; }
    public bool IsApartment { get; private set; }

    public string Description { get; private set; } = "";
    public decimal PricePerNight { get; private set; }
    public string ImagePath { get; private set; } = "";

    public Hotel Hotel { get; private set; } = null!;

    private Room()
    {
    }

    public Room(
        int roomNumber,
        int numberOfBeds,
        bool isApartment = false)
    {
        UpdateDetails(roomNumber, numberOfBeds, isApartment);
    }

    public Room(
        int hotelId,
        int roomNumber,
        int numberOfBeds,
        bool isApartment = false)
        : this(roomNumber, numberOfBeds, isApartment)
    {
        if (hotelId <= 0)
            throw new ArgumentException("Hotelul este invalid.");

        HotelId = hotelId;
    }

    public void UpdateDetails(
        int roomNumber,
        int numberOfBeds,
        bool isApartment)
    {
        if (roomNumber <= 0 || numberOfBeds is < 1 or > 8)
            throw new ArgumentException("Datele camerei sunt invalide.");

        RoomNumber = roomNumber;
        NumberOfBeds = numberOfBeds;
        IsApartment = isApartment;
    }

    public void UpdatePresentation(
        string? description,
        decimal pricePerNight,
        string? imagePath)
    {
        description = (description ?? "").Trim();
        imagePath = (imagePath ?? "").Trim();

        if (description.Length > 2000 || imagePath.Length > 240)
        {
            throw new ArgumentException(
                "Descrierea sau calea imaginii este prea lungă.");
        }

        if (pricePerNight < 0
            || pricePerNight > 99999.99m
            || decimal.Round(pricePerNight, 2) != pricePerNight)
        {
            throw new ArgumentException("Tariful este invalid.");
        }

        if (imagePath.Length > 0 && !Regex.IsMatch(
            imagePath,
            @"^/images/rooms/[A-Za-z0-9_-]+\.(?:jpg|jpeg|png|webp)$"))
        {
            throw new ArgumentException(
                "Imaginea trebuie să fie un fișier din /images/rooms/.");
        }

        Description = description;
        PricePerNight = pricePerNight;
        ImagePath = imagePath;
    }

    public string GetLabel() =>
        $"{(IsApartment ? "Apartament" : "Camera")} {RoomNumber} · "
        + $"{NumberOfBeds} {(NumberOfBeds == 1 ? "pat" : "paturi")}";

    public string GetVersion()
    {
        var data = JsonSerializer.Serialize(new
        {
            RoomId,
            HotelId,
            RoomNumber,
            NumberOfBeds,
            IsApartment,
            Description,
            PricePerNight = PricePerNight.ToString(
                "0.00",
                System.Globalization.CultureInfo.InvariantCulture),
            ImagePath
        });

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(data)));
    }
}