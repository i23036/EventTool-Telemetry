using System.Data;
using Dapper;
using ET_Backend.Models;
using ET_Backend.Models.Enums;
using FluentResults;
using Microsoft.Identity.Client;

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
                UserId = userId,
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
            var sql = $@"
            SELECT 
                a.Id               
              , a.Email      AS EMail
              , a.IsVerified
              , u.Id         AS UserId
              , u.Id         AS Id
              , u.Firstname
              , u.Lastname
              , u.Password
              , o.Id         AS OrgId
              , o.Name
              , o.Description
              , o.Domain
              , om.Role
            FROM {_db.Tbl("Accounts")}             a
            JOIN {_db.Tbl("Users")}                u  ON a.UserId       = u.Id
            JOIN {_db.Tbl("OrganizationMembers")}  om ON om.AccountId   = a.Id
            JOIN {_db.Tbl("Organizations")}        o  ON o.Id           = om.OrganizationId
            WHERE a.Email = @Email";

            if (_db.IsSQLite())
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, long, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.UserId = usr.Id;
                        acc.Organization = org;
                        acc.Organization.Id = org.Id;
                        acc.Role = (Role)(int)role;
                        return acc;
                    },
                    new { Email = accountEMail },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a)
                                                        : Result.Fail("NotFound");
            }
            else
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.UserId = usr.Id;
                        acc.Organization = org;
                        acc.Organization.Id = org.Id;
                        acc.Role = (Role)role;
                        return acc;
                    },
                    new { Email = accountEMail },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a)
                                                        : Result.Fail("NotFound");
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
            var sql = $@"
            SELECT 
                a.Id
              , a.Email      AS EMail
              , a.IsVerified
              , u.Id         AS UserId    
              , u.Id         AS Id
              , u.Firstname
              , u.Lastname
              , u.Password
              , o.Id         AS OrgId
              , o.Name
              , o.Description
              , o.Domain
              , om.Role
            FROM {_db.Tbl("Accounts")}             a
            JOIN {_db.Tbl("Users")}                u  ON a.UserId       = u.Id
            JOIN {_db.Tbl("OrganizationMembers")}  om ON om.AccountId   = a.Id
            JOIN {_db.Tbl("Organizations")}        o  ON o.Id           = om.OrganizationId
            WHERE a.Id = @Id";

            if (_db.IsSQLite())
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, long, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.UserId = usr.Id;
                        acc.Organization = org;
                        acc.Organization.Id = org.Id;
                        acc.Role = (Role)(int)role;
                        return acc;
                    },
                    new { Id = accountId },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a)
                                                        : Result.Fail("NotFound");
            }
            else
            {
                var result = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                    sql,
                    (acc, usr, org, role) =>
                    {
                        acc.User = usr;
                        acc.UserId = usr.Id;
                        acc.Organization = org;
                        acc.Organization.Id = org.Id;
                        acc.Role = (Role)role;
                        return acc;
                    },
                    new { Id = accountId },
                    splitOn: "UserId,OrgId,Role");

                return result.FirstOrDefault() is { } a ? Result.Ok(a)
                                                        : Result.Fail("NotFound");
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<List<Account>>> GetAccountsByUser(int userId)
    {
        var sql = $@"
    SELECT  a.Id, a.Email AS EMail, a.IsVerified,
            u.Id AS UserId,           -- split marker
            u.Id AS Id,               -- erste Spalte für User
            u.Firstname, u.Lastname, u.Password,
            o.Id AS OrgId, o.Name, o.Description, o.Domain,
            om.Role
    FROM    {_db.Tbl("Accounts")}            a
    JOIN    {_db.Tbl("Users")}               u  ON a.UserId = u.Id
    JOIN    {_db.Tbl("OrganizationMembers")} om ON om.AccountId = a.Id
    JOIN    {_db.Tbl("Organizations")}       o  ON o.Id      = om.OrganizationId
    WHERE   a.UserId = @Uid;";

        if (_db.IsSQLite())
        {
            var list = await _db.QueryAsync<Account, User, Models.Organization, long, Account>(
                sql,
                (acc, usr, org, role) =>
                {
                    acc.User          = usr;
                    acc.UserId        = usr.Id;
                    acc.Organization  = org; acc.Organization.Id = org.Id;
                    acc.Role          = (Role)(int)role;       // cast hier, nicht in Dapper
                    return acc;
                },
                new { Uid = userId },
                splitOn: "UserId,OrgId,Role");

            return Result.Ok(list.ToList());
        }
        else
        {
            var list = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                sql,
                (acc, usr, org, role) =>
                {
                    acc.User          = usr;
                    acc.UserId        = usr.Id;
                    acc.Organization  = org; acc.Organization.Id = org.Id;
                    acc.Role          = (Role)role;
                    return acc;
                },
                new { Uid = userId },
                splitOn: "UserId,OrgId,Role");

            return Result.Ok(list.ToList());
        }
    }
    public async Task<Result<List<Account>>> GetAccountsByMail(IEnumerable<string> mails)
    {
        var mailList = mails.Distinct().ToList();
        if (mailList.Count == 0)
            return Result.Ok(new List<Account>());

        // ► wir nutzen dieselbe join-Query wie in GetAccount, aber mit IN ()
        var sql = $@"
    SELECT  a.Id, a.Email AS EMail, a.IsVerified,
            u.Id AS UserId,
            u.Id AS Id, u.Firstname, u.Lastname, u.Password,
            o.Id AS OrgId, o.Name, o.Description, o.Domain,
            om.Role
    FROM    {_db.Tbl("Accounts")}            a
    JOIN    {_db.Tbl("Users")}               u  ON a.UserId = u.Id
    JOIN    {_db.Tbl("OrganizationMembers")} om ON om.AccountId = a.Id
    JOIN    {_db.Tbl("Organizations")}       o  ON o.Id      = om.OrganizationId
    WHERE   a.Email IN @Mails;";

        if (_db.IsSQLite())
        {
            var list = await _db.QueryAsync<Account, User, Models.Organization, long, Account>(
                sql,
                (acc, usr, org, role) =>
                {
                    acc.User = usr; acc.UserId = usr.Id;
                    acc.Organization = org; acc.Organization.Id = org.Id;
                    acc.Role = (Role)(int)role;
                    return acc;
                },
                new { Mails = mailList },
                splitOn: "UserId,OrgId,Role");

            return Result.Ok(list.ToList());
        }
        else
        {
            var list = await _db.QueryAsync<Account, User, Models.Organization, int, Account>(
                sql,
                (acc, usr, org, role) =>
                {
                    acc.User = usr; acc.UserId = usr.Id;
                    acc.Organization = org; acc.Organization.Id = org.Id;
                    acc.Role = (Role)role;
                    return acc;
                },
                new { Mails = mailList },
                splitOn: "UserId,OrgId,Role");

            return Result.Ok(list.ToList());
        }
    }

    public async Task<Result> EditAccount(Account account)
    {
        using var tx = _db.BeginSafeTransaction();
        try
        {
            // Accounts-Tabelle: nur E-Mail + Verified + Passwort
            await _db.ExecuteAsync(
                $"UPDATE {_db.Tbl("Accounts")} " +
                "SET Email = @Email, IsVerified = @IsVerified " +
                "WHERE Id = @Id;",
                new { Email = account.EMail, IsVerified = account.IsVerified ? 1 : 0, account.Id },
                tx);

            // Users-Tabelle
            await _db.ExecuteAsync(
                $"UPDATE {_db.Tbl("Users")} " +
                "SET Firstname = @Firstname, Lastname = @Lastname, Password = @Password " +
                "WHERE Id = @UserId;",
                new
                {
                    UserId = account.User.Id,
                    account.User.Firstname,
                    account.User.Lastname,
                    account.User.Password
                },
                tx);

            // OrganizationMembers  – **nur** Rolle ändern
            await _db.ExecuteAsync(
                $"UPDATE {_db.Tbl("OrganizationMembers")} " +
                "SET Role = @Role " +
                "WHERE AccountId = @AccountId;",
                new { Role = (int)account.Role, AccountId = account.Id },
                tx);

            tx.Commit();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> RemoveFromOrganization(int accountId, int orgId)
    {
        using var tx = _db.BeginSafeTransaction();
        try
        {
            // 1) UserId zum Account ermitteln
            var userId = await _db.ExecuteScalarAsync<int>(
                $"SELECT UserId FROM {_db.Tbl("Accounts")} WHERE Id = @Acc;",
                new { Acc = accountId }, tx);

            // 2) Mitgliedschaft löschen
            await _db.ExecuteAsync(
                $"DELETE FROM {_db.Tbl("OrganizationMembers")} " +
                "WHERE AccountId = @Acc;",                          
                new { Acc = accountId }, tx);

            // 3) Account löschen
            await _db.ExecuteAsync(
                $"DELETE FROM {_db.Tbl("Accounts")} WHERE Id = @Acc;",
                new { Acc = accountId }, tx);

            // 4) Prüfen, ob der User noch andere Accounts besitzt
            var remaining = await _db.ExecuteScalarAsync<int>(
                $"SELECT COUNT(1) FROM {_db.Tbl("Accounts")} WHERE UserId = @Usr;",
                new { Usr = userId }, tx);

            if (remaining == 0)
            {
                await _db.ExecuteAsync(
                    $"DELETE FROM {_db.Tbl("Users")} WHERE Id = @Usr;",
                    new { Usr = userId }, tx);
            }

            tx.Commit();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> UpdateEmail(int accountId, string email)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                $"UPDATE {_db.Tbl("Accounts")} SET Email = @Email WHERE Id = @AccId;",
                new { Email = email, AccId = accountId });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> UpdateEmailDomainsForOrganization(int orgId, string oldDomain, string newDomain)
    {
        try
        {
            var oldSuffix = "@" + oldDomain;
            var newSuffix = "@" + newDomain;

            var sql = $@"
            UPDATE {_db.Tbl("Accounts")}
            SET Email = REPLACE(Email, @OldSuffix, @NewSuffix)
            WHERE Id IN (
                SELECT AccountId
                FROM {_db.Tbl("OrganizationMembers")}
                WHERE OrganizationId = @OrgId
            );";

            await _db.ExecuteAsync(sql, new { OldSuffix = oldSuffix, NewSuffix = newSuffix, OrgId = orgId });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
    
    public async Task<Result<List<string>>> GetBoundEventNames(int accountId)
    {
        try
        {
            var sql = @"
            SELECT e.Name
            FROM Events e
            JOIN EventMembers em ON e.Id = em.EventId
            WHERE em.AccountId = @AccId
              AND (em.IsOrganizer = 1 OR em.IsContactPerson = 1 OR em.IsParticipant = 1);";

            var names = (await _db.QueryAsync<string>(sql, new { AccId = accountId })).ToList();

            return Result.Ok(names);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
}
