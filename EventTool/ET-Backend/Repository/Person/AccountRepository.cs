using System.Data;
using Dapper;
using ET_Backend.Models;

namespace ET_Backend.Repository.Person;

public class AccountRepository(IDbConnection db) : IAccountRepository
{
    private readonly IDbConnection _db = db;

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