using FluentResults;

namespace ET_Backend.Services.Helper.Authentication;
/// <summary>
/// Definiert die Authentifizierungsfunktionalität für Benutzer.
/// </summary>
public interface IAuthenticateService
{
    /// <summary>
    /// Authentifiziert einen Benutzer anhand von E-Mail und Passwort.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Benutzers.</param>
    /// <param name="password">Das Passwort des Benutzers.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit einem JWT-Token bei Erfolg oder einer Fehlermeldung bei Misserfolg.
    /// </returns>
    public Task<Result<string>> LoginUser(string eMail, string password);

    public Task<Result<string>> RegisterUser(String firstname, String lastname, String eMail, String password);

    /// <summary>
    /// Erstellt ein neues JWT für den angegebenen Account, sofern er
    /// dem aktuell angemeldeten User gehört.
    /// </summary>
    /// <param name="accountId">Ziel-Account (Mitgliedschaft).</param>
    /// <param name="currentUserId">User-Id aus dem aktuellen Token.</param>
    Task<Result<string>> SwitchAccount(int accountId, int currentUserId);

    Task<Result> AddMembership(int userId, string newEmail);
}