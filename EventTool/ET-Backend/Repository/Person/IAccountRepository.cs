using ET_Backend.Models;

namespace ET_Backend.Repository.Person;

/// <summary>
/// Definiert Methoden für den Zugriff auf Kontodaten in der Datenquelle.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Prüft, ob ein Konto mit der angegebenen E-Mail-Adresse existiert.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse, nach der gesucht wird.</param>
    /// <returns>Ein Task mit einem booleschen Wert: true, wenn das Konto existiert.</returns>
    public Task<bool> AccountExists(String eMail);


	public Task<Account> CreateAccount(String eMail, Models.Organization organization, Role role);

    /// <summary>
    /// Ruft das Konto zur angegebenen E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task mit dem <see cref="Account"/>-Objekt.</returns>
    public Task<Account> GetAccount(String eMail);

    /// <summary>
    /// Ruft den Passwort-Hash des Kontos anhand der E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task mit dem Passwort-Hash als Zeichenkette.</returns>
    public Task<String> GetPasswordHash(String eMail);
}