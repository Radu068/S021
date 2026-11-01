namespace HotelManager.Models;

public class Receptionist : User
{
    public override string Role => nameof(Receptionist);

    private Receptionist()
    {
    }

    public Receptionist(string firstName, string lastName, string email)
        : base(firstName, lastName, email)
    {
    }
}