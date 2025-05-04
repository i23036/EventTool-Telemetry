using System.Data;
using Dapper;
using ET_Backend.Models;

namespace ET_Backend.Repository.Person;
/// <summary>
/// Implementierung des Repositories für den Zugriff auf Kontodaten.
/// </summary>

public class AccountRepository(IDbConnection db) : IAccountRepository
{
    private readonly IDbConnection _db = db;

    /// <summary>
    /// Prüft, ob ein Konto mit der angegebenen E-Mail-Adresse existiert.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse, nach der gesucht werden soll.</param>
    /// <returns>Ein Task mit einem booleschen Ergebnis: true, wenn das Konto existiert.</returns>
    public async Task<bool> AccountExists(String eMail)
    {
        const string sql = @"
                SELECT COUNT(1)
                FROM Accounts
                WHERE Email = @EMail;
            ";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { EMail = eMail });
        return count > 0;
    }


    public Task<bool> CreateAccount(String eMail, Models.Organization organization, Role role)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ruft das Konto zur angegebenen E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task, der das entsprechende <see cref="Account"/>-Objekt zurückgibt.</returns>
    public async Task<Account> GetAccount(string eMail)
    {
        const string sql = @"
                SELECT
                    Email   AS EMail,
                    Organization,
                    Role
                FROM Accounts
                WHERE Email = @EMail;
            ";
        return await _db.QuerySingleOrDefaultAsync<Account>(sql, new { EMail = eMail });
    }

    /// <summary>
    /// Ruft den Passwort-Hash eines Kontos anhand der E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task mit dem Passwort-Hash als Zeichenkette.</returns>
    public async Task<string> GetPasswordHash(string eMail)
    {
        const string sql = @"
                SELECT PasswordHash
                FROM Accounts
                WHERE Email = @EMail;
            ";
        return await _db.ExecuteScalarAsync<string>(sql, new { EMail = eMail });
    }
}