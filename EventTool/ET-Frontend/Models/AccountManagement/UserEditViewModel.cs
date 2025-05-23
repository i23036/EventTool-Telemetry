using System.ComponentModel.DataAnnotations;

namespace ET_Frontend.Models.AccountManagement;

public class UserEditViewModel
{
    [Required(ErrorMessage = "Vorname ist erforderlich.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Nachname ist erforderlich.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Passwort ist erforderlich.")]
    [MinLength(6, ErrorMessage = "Passwort muss mindestens 6 Zeichen lang sein.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Wiederholtes Passwort ist erforderlich.")]
    [Compare(nameof(Password), ErrorMessage = "Passwörter stimmen nicht überein.")]
    public string Reppassword { get; set; }
}