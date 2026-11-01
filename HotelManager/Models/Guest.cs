namespace HotelManager.Models;

public class Guest : User
{
    public override string Role => nameof(Guest);

    private Guest()
    {
    }

    public Guest(string firstName, string lastName, string email)
        : base(firstName, lastName, email)
    {
    }
}