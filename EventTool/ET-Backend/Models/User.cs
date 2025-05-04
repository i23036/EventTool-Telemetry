namespace ET_Backend.Models;
/// <summary>
/// Repräsentiert einen Benutzer mit Namen und Passwort.
/// </summary>
public class User
{
    /// <summary>
    /// Der Nachname des Benutzers.
    /// </summary>
    public String Name { set; get; }

    /// <summary>
    /// Der Vorname des Benutzers.
    /// </summary>
    public String FirstName { set; get; }

    /// <summary>
    /// Das Passwort des Benutzers.
    /// </summary>
    public String Password { set; get; }
}