using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Person;

/// <summary>
/// Repository für den Zugriff auf Accounts / Users – funktioniert sowohl mit
/// SQLite (Dev) als auch mit Azure SQL (Prod). Plattformabhängige Details
/// (Tabellennamen, Identity‑Abruf) werden zur Laufzeit ermittelt.
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
            var count = await _db.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM {_db.Tbl("Accounts")} WHERE Email = @Email", new { Email = accountEMail });
            return Result.Ok(count > 0);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AccountExists(int accountId)
    {
        try
        {
            var count = await _db.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM {_db.Tbl("Accounts")} WHERE Id = @Id", new { Id = accountId });
            return Result.Ok(count > 0);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task<Result<Account>> CreateAccount(string accountEMail, Models.Organization organization, Role role, User user)
    {
        using var tx = _db.BeginSafeTransaction();
        try
        {
            // 1) User
            var userInsert = $"INSERT INTO {_db.Tbl("Users")}(Firstname, Lastname, Password) VALUES (@Firstname, @Lastname, @Password)";
            var userId = await _db.InsertAndGetIdAsync(userInsert, new { user.Firstname, user.Lastname, user.Password }, tx);
            user.Id = userId;

            // 2) Account
            var accInsert = $"INSERT INTO {_db.Tbl("Accounts")}(Email, UserId) VALUES (@Email, @UserId)";
            var accountId = await _db.InsertAndGetIdAsync(accInsert, new { Email = accountEMail, UserId = userId }, tx);

            // 3) Org‑Member
            await _db.ExecuteAsync($"INSERT INTO {_db.Tbl("OrganizationMembers")}(AccountId, OrganizationId, Role) VALUES (@AccountId, @OrganizationId, @Role);",
                                   new { AccountId = accountId, OrganizationId = organization.Id, Role = (int)role }, tx);

            tx.Commit();
            
            var account = new Account
            {
                Id = accountId,
                EMail = accountEMail,
                IsVerified = false,
                User = user,
                Organization = organization,
                Role = role
            };
            return Result.Ok(account);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAccount(string accountEMail)
    {
        try
        {
            var rows = await _db.ExecuteAsync($"DELETE FROM {_db.Tbl("Accounts")} WHERE Email = @Email", new { Email = accountEMail });
            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAccount(int accountId)
    {
        try
        {
            var rows = await _db.ExecuteAsync($"DELETE FROM {_db.Tbl("Accounts")} WHERE Id = @Id", new { Id = accountId });
            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
    
    public async Task<Result<Account>> GetAccount(string accountEMail)
    {
        try
        {
            var sql = $@"SELECT 
                            a.Id              AS AccountId,
                            a.Email           AS EMail,
                            a.IsVerified,
                            u.Id              AS UserId,
                            u.Firstname,
                            u.Lastname,
                            u.Password,
                            o.Id              AS OrgId,
                            o.Name            AS Name,
                            o.Description,
                            o.Domain,
                            om.Role
                          FROM {_db.Tbl("Accounts")} a
                          JOIN {_db.Tbl("Users")} u             ON a.UserId       = u.Id
                          JOIN {_db.Tbl("OrganizationMembers")} om ON om.AccountId   = a.Id
                          JOIN {_db.Tbl("Organizations")} o     ON o.Id           = om.OrganizationId
                          WHERE a.Email = @Email";

            if (_db.IsSQLite())
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, long, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.Organization = org;
                        acc.Role = (Role)(int)role; // cast long → int → enum
                        return acc;
                    },
                    new { Email = accountEMail },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a) : Result.Fail("NotFound");
            }
            else
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.Organization = org;
                        acc.Role = (Role)role;
                        return acc;
                    },
                    new { Email = accountEMail },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a) : Result.Fail("NotFound");
            }

        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Account>> GetAccount(int accountId)
    {
        try
        {
            var sql = $@"SELECT 
                            a.Id              AS AccountId,
                            a.Email           AS EMail,
                            a.IsVerified,
                            u.Id              AS UserId,
                            u.Firstname,
                            u.Lastname,
                            u.Password,
                            o.Id              AS OrgId,
                            o.Name            AS Name,
                            o.Description,
                            o.Domain,
                            om.Role
                          FROM {_db.Tbl("Accounts")} a
                          JOIN {_db.Tbl("Users")} u             ON a.UserId       = u.Id
                          JOIN {_db.Tbl("OrganizationMembers")} om ON om.AccountId   = a.Id
                          JOIN {_db.Tbl("Organizations")} o     ON o.Id           = om.OrganizationId
                          WHERE a.Id = @Id";

            if (_db.IsSQLite())
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, long, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.Organization = org;
                        acc.Role = (Role)(int)role; // cast long → int → enum
                        return acc;
                    },
                    new { Id = accountId },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a) : Result.Fail("NotFound");
            }
            else
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.Organization = org;
                        acc.Role = (Role)role;
                        return acc;
                    },
                    new { Id = accountId },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a) : Result.Fail("NotFound");
            }

        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> EditAccount(Account account)
    {
        using var tx = _db.BeginSafeTransaction();
        try
        {
            // Account
            await _db.ExecuteAsync($"UPDATE {_db.Tbl("Accounts")} SET Email = @Email, IsVerified = @IsVerified WHERE Id = @Id;",
                                   new { Email = account.EMail, IsVerified = account.IsVerified ? 1 : 0, account.Id }, tx);

            // User
            await _db.ExecuteAsync($"UPDATE {_db.Tbl("Users")} SET Firstname = @Firstname, Lastname = @Lastname, Password = @Password WHERE Id = @UserId;",
                                   new { UserId = account.User.Id, account.User.Firstname, account.User.Lastname, account.User.Password }, tx);

            // Org‑Member
            await _db.ExecuteAsync($"UPDATE {_db.Tbl("OrganizationMembers")} SET Role = @Role WHERE AccountId = @AccountId AND OrganizationId = @OrganizationId;",
                                   new { Role = (int)account.Role, AccountId = account.Id, OrganizationId = account.Organization.Id }, tx);

            tx.Commit();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> RemoveFromOrganization(int accountId)
    {
        try
        {
            var sql = "UPDATE Accounts SET OrganizationId = NULL WHERE Id = @Id";
            var rows = await _db.ExecuteAsync(sql, new { Id = accountId });

            return rows > 0 ? Result.Ok() : Result.Fail("No changes");
        }
        catch
        {
            return Result.Fail("DBError");
        }
    }
}
