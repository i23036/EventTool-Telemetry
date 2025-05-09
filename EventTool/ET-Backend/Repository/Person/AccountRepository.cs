using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

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
    public async Task<Result<bool>> AccountExists(String eMail)
    {
        try
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Accounts
                WHERE Email = @EMail;
            ";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { EMail = eMail });
            return Result.Ok(count > 0);
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }


    public async Task<Result> CreateAccount(String eMail, Models.Organization organization, Role role)
    {
        try
        {
            throw new NotImplementedException();
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }

    /// <summary>
    /// Ruft das Konto zur angegebenen E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task, der das entsprechende <see cref="Account"/>-Objekt zurückgibt.</returns>
    public async Task<Result<Account>> GetAccount(string eMail)
    {
        try
        {
            const string sql = @"
                SELECT
                    Email   AS EMail,
                    Organization,
                    Role
                FROM Accounts
                WHERE Email = @EMail;
            ";
            return Result.Ok(await _db.QuerySingleOrDefaultAsync<Account>(sql, new { EMail = eMail }));
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }

    /// <summary>
    /// Ruft den Passwort-Hash eines Kontos anhand der E-Mail-Adresse ab.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Kontos.</param>
    /// <returns>Ein Task mit dem Passwort-Hash als Zeichenkette.</returns>
    public async Task<Result<string>> GetPasswordHash(string eMail)
    {
        try
        {
            const string sql = @"
                SELECT PasswordHash
                FROM Accounts
                WHERE Email = @EMail;
            ";
            return Result.Ok(await _db.ExecuteScalarAsync<string>(sql, new { EMail = eMail }));
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }
}