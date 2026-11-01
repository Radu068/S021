using System.ComponentModel.DataAnnotations;

namespace HotelManager.Models;

public abstract class User
{
    public int UserId { get; private set; }
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string NormalizedEmail { get; private set; } = "";
    public string PasswordHash { get; private set; } = "";

    public string SecurityStamp { get; private set; }
        = Guid.NewGuid().ToString("N");

    public abstract string Role { get; }

    protected User() { }

    protected User(
        string firstName,
        string lastName,
        string email) =>
        UpdateProfile(firstName, lastName, email);

    public string GetFullName() =>
        $"{FirstName} {LastName}".Trim();

    public void UpdateProfile(
        string firstName,
        string lastName,
        string email)
    {
        firstName = (firstName ?? "").Trim();
        lastName = (lastName ?? "").Trim();
        email = (email ?? "").Trim();

        if (firstName.Length is < 1 or > 50 ||
            lastName.Length is < 1 or > 50 ||
            email.Length > 254 ||
            !new EmailAddressAttribute().IsValid(email))
        {
            throw new ArgumentException(
                "Datele profilului sunt invalide.");
        }

        if (FirstName == firstName &&
            LastName == lastName &&
            Email == email)
        {
            return;
        }

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }
}