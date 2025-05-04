using ET_Backend.Models;

namespace ET_Backend.Repository.Person;
/// <summary>
/// Implementierung des Repositories für den Zugriff auf Kontodaten.
/// </summary>
public class AccountRepository : IAccountRepository
{
    /// <summary>
    /// Prüft, ob ein Konto mit der angegebenen E-Mail-Adresse existiert.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse, nach der gesucht werden soll.</param>
    /// <returns>Ein Task mit einem booleschen Ergebnis: true, wenn das Konto existiert.</returns>
    public Task<bool> AccountExists(String eMail)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ruft das Konto zur angegebenen E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task, der das entsprechende <see cref="Account"/>-Objekt zurückgibt.</returns>
    public Task<Account> GetAccount(String eMail)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ruft den Passwort-Hash eines Kontos anhand der E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task mit dem Passwort-Hash als Zeichenkette.</returns>
    public Task<String> GetPasswordHash(String eMail)
    {
        throw new NotImplementedException();
    }
}