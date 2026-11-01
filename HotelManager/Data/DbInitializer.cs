using System.ComponentModel.DataAnnotations;
using HotelManager.Models;
using HotelManager.Models.Auth;
using HotelManager.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var db = services.GetRequiredService<AppDbContext>();

        if (!await db.Hotels.AnyAsync())
        {
            var hotel = new Hotel("HotelManager");

            for (int number = 101; number <= 106; number++)
                hotel.AddRoom(new Room(number, 2));

            hotel.AddRoom(new Room(201, 4, true));
            hotel.AddRoom(new Room(202, 4, true));

            db.Hotels.Add(hotel);
            await db.SaveChangesAsync();
        }

        var demo =
            new Dictionary<int, (decimal Price, string Description, string File)>
            {
                [101] = (
                    220m,
                    "Cameră cu două paturi, pentru un sejur simplu și confortabil.",
                    "camera-101.jpg"),

                [102] = (
                    240m,
                    "Cameră cu două paturi. Alege perioada potrivită pentru ședere.",
                    "camera-102.jpg"),

                [103] = (
                    260m,
                    "Cameră cu două paturi, disponibilă pentru rezervare online.",
                    "camera-103.jpg"),

                [104] = (
                    280m,
                    "Cameră cu două paturi, pentru următoarea ta călătorie.",
                    "camera-104.jpg"),

                [105] = (
                    300m,
                    "Cameră cu două paturi. Consultă disponibilitatea pentru datele alese.",
                    "camera-105.jpg"),

                [106] = (
                    320m,
                    "Cameră cu două paturi, pentru un sejur bine organizat.",
                    "camera-106.jpg"),

                [201] = (
                    450m,
                    "Apartament cu patru paturi, potrivit pentru un sejur în grup.",
                    "apartament-201.jpg"),

                [202] = (
                    500m,
                    "Apartament cu patru paturi. Alege datele și pregătește următorul sejur.",
                    "apartament-202.jpg")
            };

        var demoRooms = await db.Rooms
            .Where(x => x.Hotel.Name == "HotelManager")
            .ToListAsync();

        foreach (var room in demoRooms)
        {
            if (demo.TryGetValue(room.RoomNumber, out var details)
                && room.PricePerNight == 0
                && string.IsNullOrWhiteSpace(room.Description)
                && string.IsNullOrWhiteSpace(room.ImagePath))
            {
                room.UpdatePresentation(
                    details.Description,
                    details.Price,
                    "/images/rooms/" + details.File);
            }
        }

        await db.SaveChangesAsync();

        var email =
            configuration["SeedReceptionist:Email"]?.Trim();

        var password =
            configuration["SeedReceptionist:Password"];

        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Configurează emailul și parola recepționerului.");
        }

        var input = new RegisterInput
        {
            FirstName = "Radu",
            LastName = "Nadolu",
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        Validator.ValidateObject(
            input,
            new ValidationContext(input),
            true);

        var users =
            services.GetRequiredService<IUserRepository>();

        var existing = await users.FindByEmailAsync(
            email.ToUpperInvariant());

        if (existing is Receptionist)
            return;

        if (existing is not null)
        {
            throw new InvalidOperationException(
                "Emailul recepționerului aparține deja unui client.");
        }

        var receptionist = new Receptionist(
            input.FirstName,
            input.LastName,
            input.Email);

        receptionist.SetPasswordHash(
            services.GetRequiredService<IPasswordHasher<User>>()
                .HashPassword(receptionist, password));

        if (!await users.TryAddAsync(receptionist))
        {
            throw new InvalidOperationException(
                "Contul de recepție nu a putut fi creat.");
        }
    }
}