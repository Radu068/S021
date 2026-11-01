using System.ComponentModel.DataAnnotations;

namespace HotelManager.Models.Auth;

public class LoginInput
{
    [Required(ErrorMessage = "Emailul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Introdu un email cu format valid.")]
    [StringLength(254), Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [StringLength(128), DataType(DataType.Password), Display(Name = "Parolă")]
    public string Password { get; set; } = "";
}