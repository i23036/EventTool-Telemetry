using System.Data;
using Dapper;
using FluentResults;

namespace ET_Backend.Repository.Authentication;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly IDbConnection _db;

    public EmailVerificationTokenRepository(IDbConnection db) => _db = db;

    public async Task<Result> CreateAsync(int accountId, string token)
    {
        const string sql = @"
            INSERT INTO EmailVerificationTokens (AccountId, Token, ExpiresAt)
            VALUES (@Acc, @Tok, @Exp);
        ";
        try
        {
            var expiresAt = DateTime.UtcNow.AddDays(2);
            await _db.ExecuteAsync(sql, new { Acc = accountId, Tok = token, Exp = expiresAt });
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"[DEBUG] Token-Insert fehlgeschlagen: {ex.Message}");
        }
    }

    public async Task<Result<(int AccountId, DateTime ExpiresAt)>> GetAsync(string token)
    {
        const string sql = @"
            SELECT AccountId, ExpiresAt
            FROM EmailVerificationTokens
            WHERE Token = @Tok;
        ";
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<(int, DateTime)>(sql, new { Tok = token });
            return result == default
                ? Result.Fail("Token nicht gefunden.")
                : Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Token-Suche fehlgeschlagen: {ex.Message}");
        }
    }

    public async Task<Result> ConsumeAsync(string token, IDbConnection conn, IDbTransaction tr)
    {
        try
        {
            var rows = await conn.ExecuteAsync(
                "DELETE FROM EmailVerificationTokens WHERE Token = @Tok",
                new { Tok = token }, tr);

            return rows > 0
                ? Result.Ok()
                : Result.Fail("Token wurde nicht gefunden oder konnte nicht gelöscht werden.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Fehler beim Löschen des Tokens: {ex.Message}");
        }
    }
}