using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

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
            var userId = await _db.ExecuteScalarAsync<int>(@"
                INSERT INTO Users (Firstname, Lastname, Password)
                VALUES (@Firstname, @Lastname, @Password);
                SELECT CAST(SCOPE_IDENTITY() AS int);",
                new
                {
                    Firstname = firstname,
                    Lastname = lastname,
                    Password = password
                });

            return await GetUser(userId);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUser(int userId)
    {
        try
        {
            var affected = await _db.ExecuteAsync(
                "DELETE FROM Users WHERE Id = @Id",
                new { Id = userId });

            return affected > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch(Exception ex)
        {
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
