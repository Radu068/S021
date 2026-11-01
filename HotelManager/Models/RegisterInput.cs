using System.ComponentModel.DataAnnotations;

namespace HotelManager.Models.Auth;

public class RegisterInput
{
    [Required(ErrorMessage = "Prenumele este obligatoriu.")]
    [StringLength(50, ErrorMessage = "Prenumele poate avea maximum 50 de caractere.")]
    [Display(Name = "Prenume")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [StringLength(50, ErrorMessage = "Numele poate avea maximum 50 de caractere.")]
    [Display(Name = "Nume")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Emailul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Introdu o adresă de email validă.")]
    [StringLength(254, ErrorMessage = "Emailul este prea lung.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [StringLength(
        128,
        MinimumLength = 15,
        ErrorMessage = "Parola trebuie să aibă între 15 și 128 de caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmarea parolei este obligatorie.")]
    [StringLength(128)]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Parolele nu coincid.")]
    [Display(Name = "Confirmă parola")]
    public string ConfirmPassword { get; set; } = string.Empty;
}