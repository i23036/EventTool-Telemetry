using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;
using Microsoft.Data.Sqlite;

namespace ET_Backend.Repository.Person;

/// <summary>
/// Implementierung des Repositories für den Zugriff auf Benutzerdaten.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnection _db;

    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Result<bool>> UserExists(int userId)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Users WHERE Id = @Id",
                new { Id = userId });

            return Result.Ok(exists);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<User>> CreateUser(string firstname, string lastname, string password)
    {
        try
        {
            var insertSql = @"
            INSERT INTO Users (Firstname, Lastname, Password)
            VALUES (@Firstname, @Lastname, @Password);";

            // Unterscheide automatisch SQLite vs. SQL Server
            var idQuery = _db is SqliteConnection
                ? "SELECT last_insert_rowid();"
                : "SELECT CAST(SCOPE_IDENTITY() AS int);";

            var userId = await _db.ExecuteScalarAsync<int>(
                insertSql + idQuery,
                new
                {
                    Firstname = firstname,
                    Lastname = lastname,
                    Password = password
                });

            return await GetUser(userId);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
    
    public async Task<Result> DeleteUser(int userId)
    {
        using var tx = _db.BeginSafeTransaction();    

        try
        {
            // Alle Accounts des Users ermitteln
            var accountIds = (await _db.QueryAsync<int>(
                $"SELECT Id FROM {_db.Tbl("Accounts")} WHERE UserId = @UserId",
                new { UserId = userId }, tx)).ToList();

            if (!accountIds.Any())
            {
                tx.Rollback();
                return Result.Fail("NotFound");
            }

            // Mitgliedschaften entfernen
            await _db.ExecuteAsync(
                $"DELETE FROM {_db.Tbl("OrganizationMembers")} WHERE AccountId IN @AccIds",
                new { AccIds = accountIds }, tx);

            // Event-Teilnahmen entfernen
            await _db.ExecuteAsync(
                $"DELETE FROM {_db.Tbl("EventMembers")} WHERE AccountId IN @AccIds",
                new { AccIds = accountIds }, tx);

            // Accounts löschen
            await _db.ExecuteAsync(
                $"DELETE FROM {_db.Tbl("Accounts")} WHERE UserId = @UserId",
                new { UserId = userId }, tx);

            // User löschen
            await _db.ExecuteAsync(
                $"DELETE FROM {_db.Tbl("Users")} WHERE Id = @UserId",
                new { UserId = userId }, tx);

            tx.Commit();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
    
    public async Task<Result<User>> GetUser(int userId)
    {
        try
        {
            var user = await _db.QueryFirstOrDefaultAsync<User>(
                "SELECT Id, Firstname, Lastname, Password FROM Users WHERE Id = @Id",
                new { Id = userId });

            return user == null ? Result.Fail("NotFound") : Result.Ok(user);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> EditUser(User user)
    {
        try
        {
            var rows = await _db.ExecuteAsync(@"
                UPDATE Users
                SET Firstname = @Firstname,
                    Lastname = @Lastname,
                    Password = @Password
                WHERE Id = @Id;",
                new
                {
                    user.Firstname,
                    user.Lastname,
                    user.Password,
                    user.Id
                });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
}
