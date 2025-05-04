using ET_Backend.Models;

namespace ET_Backend.Repository.Person;
/// <summary>
/// Definiert Methoden zum Zugriff auf Benutzerdaten in der Datenquelle.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Prüft, ob ein Benutzer mit der angegebenen ID existiert.
    /// </summary>
    /// <param name="id">Die ID des Benutzers.</param>
    /// <returns>Ein Task mit true, wenn der Benutzer existiert.</returns>
    public Task<bool> UserExists(int id);

    /// <summary>
    /// Erstellt einen neuen Benutzer mit Vorname, Nachname und Passwort.
    /// </summary>
    /// <param name="firstname">Der Vorname des Benutzers.</param>
    /// <param name="lastname">Der Nachname des Benutzers.</param>
    /// <param name="password">Das Passwort des Benutzers (ungehasht).</param>
    /// <returns>Ein Task mit true, wenn der Benutzer erfolgreich erstellt wurde.</returns>
    public Task<bool> CreateUser(String firstname, String lastname, String password);

    /// <summary>
    /// Ruft die Benutzerdaten zur angegebenen ID ab.
    /// </summary>
    /// <param name="id">Die ID des Benutzers.</param>
    /// <returns>Ein Task mit dem entsprechenden <see cref="User"/>-Objekt.</returns>
    public Task<User> GetUser(int id);

    /// <summary>
    /// Ruft den Passwort-Hash eines Benutzers anhand seiner ID ab.
    /// </summary>
    /// <param name="id">Die ID des Benutzers.</param>
    /// <returns>Ein Task mit dem Passwort-Hash als Zeichenkette.</returns>
    public Task<String> GetPasswordHash(int id);
}