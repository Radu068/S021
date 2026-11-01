using System.ComponentModel.DataAnnotations;
using HotelManager.Models;
using HotelManager.Models.Auth;
using HotelManager.Repositories.Interfaces;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HotelManager.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _passwordHasher;

    private static readonly Guest DummyUser =
        new("Utilizator", "Necunoscut", "unknown@example.invalid");

    private static readonly string DummyHash =
        new PasswordHasher<User>().HashPassword(
            DummyUser,
            Guid.NewGuid().ToString("N"));

    public AuthService(
        IUserRepository users,
        IPasswordHasher<User> passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, string? Error)> RegisterGuestAsync(
        RegisterInput input)
    {
        input.FirstName = (input.FirstName ?? string.Empty).Trim();
        input.LastName = (input.LastName ?? string.Empty).Trim();
        input.Email = (input.Email ?? string.Empty).Trim();

        var errors = new List<ValidationResult>();

        if (!Validator.TryValidateObject(
                input,
                new ValidationContext(input),
                errors,
                validateAllProperties: true))
        {
            return (
                false,
                errors.FirstOrDefault()?.ErrorMessage
                    ?? "Datele introduse sunt invalide.");
        }

        const string duplicateEmail =
            "Există deja un cont cu această adresă de email.";

        string normalizedEmail = input.Email.ToUpperInvariant();

        if (await _users.FindByEmailAsync(normalizedEmail) is not null)
        {
            return (false, duplicateEmail);
        }

        var guest = new Guest(
            input.FirstName,
            input.LastName,
            input.Email);

        guest.SetPasswordHash(
            _passwordHasher.HashPassword(guest, input.Password));

        bool created = await _users.TryAddAsync(guest);

        return created
            ? (true, null)
            : (false, duplicateEmail);
    }

    public async Task<User?> AuthenticateAsync(LoginInput input)
    {
        input.Email = (input.Email ?? string.Empty).Trim();

        if (!Validator.TryValidateObject(
                input,
                new ValidationContext(input),
                new List<ValidationResult>(),
                validateAllProperties: true))
        {
            return null;
        }

        var user = await _users.FindByEmailAsync(
            input.Email.ToUpperInvariant());

        // Verificăm un hash și pentru un email inexistent.
        var verification = _passwordHasher.VerifyHashedPassword(
            user ?? DummyUser,
            user?.PasswordHash ?? DummyHash,
            input.Password);

        if (user is null ||
            verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verification ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.SetPasswordHash(
                _passwordHasher.HashPassword(user, input.Password));

            await _users.SaveChangesAsync();
        }

        return user;
    }

    public Task<User?> GetUserByIdAsync(int userId)
    {
        return _users.FindByIdAsync(userId);
    }
}