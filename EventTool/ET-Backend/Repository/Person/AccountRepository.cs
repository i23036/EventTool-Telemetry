using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Person;

/// <summary>
/// Implementierung des Repositories für den Zugriff auf Kontodaten.
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly IDbConnection _db;

    public AccountRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Result<bool>> AccountExists(string accountEMail)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Accounts WHERE Email = @Email",
                new { Email = accountEMail });

            return Result.Ok(exists);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AccountExists(int accountId)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Accounts WHERE Id = @Id",
                new { Id = accountId });

            return Result.Ok(exists);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Account>> CreateAccount(string accountEMail, Models.Organization organization, Role role, User user)
    {
        using var tx = _db.BeginTransaction();
        try
        {
            var userId = await _db.ExecuteScalarAsync<int>(@"
                INSERT INTO Users (Firstname, Lastname, Password)
                VALUES (@Firstname, @Lastname, @Password);
                SELECT last_insert_rowid();",
                new
                {
                    user.Firstname,
                    user.Lastname,
                    user.Password
                }, tx);

            var accountId = await _db.ExecuteScalarAsync<int>(@"
                INSERT INTO Accounts (Email, UserId)
                VALUES (@Email, @UserId);
                SELECT last_insert_rowid();",
                new
                {
                    Email = accountEMail,
                    UserId = userId
                }, tx);

            await _db.ExecuteAsync(@"
                INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role)
                VALUES (@AccountId, @OrgId, @Role);",
                new
                {
                    AccountId = accountId,
                    OrgId = organization.Id,
                    Role = (int)role
                }, tx);

            tx.Commit();
            return await GetAccount(accountId);
        }
        catch(Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAccount(string accountEMail)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM Accounts WHERE Email = @Email",
                new { Email = accountEMail });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAccount(int accountId)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM Accounts WHERE Id = @Id",
                new { Id = accountId });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Account>> GetAccount(string accountEMail)
    {
        try
        {
            var sql = @"
                SELECT 
                    a.Id, a.Email AS EMail, a.IsVerified, 
                    u.Id, u.Firstname, u.Lastname, u.Password,
                    o.Id, o.Name AS Name, o.Description, o.Domain,
                    om.Role
                FROM Accounts a
                JOIN Users u ON a.UserId = u.Id
                JOIN OrganizationMembers om ON om.AccountId = a.Id
                JOIN Organizations o ON o.Id = om.OrganizationId
                WHERE a.Email = @Email";

            var account = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                sql,
                (acc, user, org, role) =>
                {
                    acc.User = user;
                    acc.Organization = org;
                    acc.Role = (Role)role;
                    return acc;
                },
                new { Email = accountEMail },
                splitOn: "Id,Id,Role");

            var result = account.FirstOrDefault();
            return result == null ? Result.Fail("NotFound") : Result.Ok(result);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Account>> GetAccount(int accountId)
    {
        try
        {
            var sql = @"
                SELECT 
                    a.Id, a.Email AS EMail, a.IsVerified, 
                    u.Id, u.Firstname, u.Lastname, u.Password,
                    o.Id, o.Name AS Name, o.Description, o.Domain,
                    om.Role
                FROM Accounts a
                JOIN Users u ON a.UserId = u.Id
                JOIN OrganizationMembers om ON om.AccountId = a.Id
                JOIN Organizations o ON o.Id = om.OrganizationId
                WHERE a.Id = @Id";

            var account = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                sql,
                (acc, user, org, role) =>
                {
                    acc.User = user;
                    acc.Organization = org;
                    acc.Role = (Role)role;
                    return acc;
                },
                new { Id = accountId },
                splitOn: "Id,Id,Role");

            var result = account.FirstOrDefault();
            return result == null ? Result.Fail("NotFound") : Result.Ok(result);
        }
        catch(Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> EditAccount(Account account)
    {
        using var tx = _db.BeginTransaction();
        try
        {
            await _db.ExecuteAsync(@"
                UPDATE Accounts
                SET Email = @Email,
                    IsVerified = @IsVerified
                WHERE Id = @Id;",
                new
                {
                    Email = account.EMail,
                    IsVerified = account.IsVerified ? 1 : 0,
                    account.Id
                }, tx);

            await _db.ExecuteAsync(@"
                UPDATE Users
                SET Firstname = @Firstname,
                    Lastname = @Lastname,
                    Password = @Password
                WHERE Id = @UserId;",
                new
                {
                    UserId = account.User.Id,
                    account.User.Firstname,
                    account.User.Lastname,
                    account.User.Password
                }, tx);

            await _db.ExecuteAsync(@"
                UPDATE OrganizationMembers
                SET Role = @Role
                WHERE AccountId = @AccountId AND OrganizationId = @OrganizationId;",
                new
                {
                    Role = (int)account.Role,
                    AccountId = account.Id,
                    OrganizationId = account.Organization.Id
                }, tx);

            tx.Commit();
            return Result.Ok();
        }
        catch(Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
}
