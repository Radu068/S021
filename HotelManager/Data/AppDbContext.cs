using HotelManager.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<User>();

        user.ToTable("Users");
        user.HasKey(x => x.UserId);

        user.Property(x => x.FirstName)
            .HasMaxLength(50).IsRequired();
        user.Property(x => x.LastName)
            .HasMaxLength(50).IsRequired();
        user.Property(x => x.Email)
            .HasMaxLength(254).IsRequired();
        user.Property(x => x.NormalizedEmail)
            .HasMaxLength(254).IsRequired();
        user.Property(x => x.PasswordHash)
            .HasMaxLength(512).IsRequired();
        user.Property(x => x.SecurityStamp)
            .HasMaxLength(32).IsRequired();

        user.HasIndex(x => x.NormalizedEmail).IsUnique();
        user.Ignore(x => x.Role);

        user.HasDiscriminator<string>("UserType")
            .HasValue<Guest>(nameof(Guest))
            .HasValue<Receptionist>(nameof(Receptionist));

        var hotel = modelBuilder.Entity<Hotel>();

        hotel.Property(x => x.Name)
            .HasMaxLength(100).IsRequired();

        hotel.Ignore(x => x.NumberOfRooms);
        hotel.Ignore(x => x.NumberOfApartments);

        var room = modelBuilder.Entity<Room>();

        room.HasIndex(x => new { x.HotelId, x.RoomNumber })
            .IsUnique();

        room.HasOne(x => x.Hotel)
            .WithMany(x => x.Rooms)
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        var reservation = modelBuilder.Entity<Reservation>();

        reservation.ToTable("Reservations", table =>
        {
            table.HasCheckConstraint(
                "CK_Reservation_Period",
                "\"CheckOut\" > \"CheckIn\"");

            table.HasCheckConstraint(
                "CK_Reservation_Status",
                "\"Status\" BETWEEN 0 AND 3");
        });

        reservation.Property(x => x.Version)
            .HasMaxLength(32)
            .IsConcurrencyToken();

        reservation.HasIndex(x =>
            new { x.RoomId, x.CheckIn, x.CheckOut });

        reservation.HasOne(x => x.Guest)
            .WithMany()
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        reservation.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        reservation.HasOne(x => x.Receptionist)
            .WithMany()
            .HasForeignKey(x => x.ReceptionistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}