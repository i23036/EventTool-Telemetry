using System.Data;
using Dapper;
using ET.Shared.DTOs;
using FluentResults;
using ET_Backend.Models.Enums;

namespace ET_Backend.Repository.Organization;

/// <summary>
/// Dapper-basierte Implementierung für den Datenzugriff auf Organisationen.
/// </summary>
public class OrganizationRepository : IOrganizationRepository
{
    private readonly IDbConnection _db;

    public OrganizationRepository(IDbConnection db)
    {
        _db = db;
    }

    // === Existenzprüfung ===

    public async Task<Result<bool>> OrganizationExists(string domain)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Organizations WHERE Domain = @Domain",
                new { Domain = domain });

            return Result.Ok(exists);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<bool>> OrganizationExists(int id)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Organizations WHERE Id = @Id",
                new { Id = id });

            return Result.Ok(exists);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    // === Lesen ===

    public async Task<Result<List<Models.Organization>>> GetAllOrganizations()
    {
        try
        {
            var orgs = (await _db.QueryAsync<Models.Organization>(
                "SELECT Id, Name, Description, Domain, OrgaPicAsBase64 FROM Organizations")).ToList();

            return Result.Ok(orgs);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Models.Organization>> GetOrganization(string domain)
    {
        try
        {
            var org = await _db.QuerySingleOrDefaultAsync<Models.Organization>(
                "SELECT Id, Name, Description, Domain, OrgaPicAsBase64 FROM Organizations WHERE Domain = @Domain",
                new { Domain = domain });

            return org != null ? Result.Ok(org) : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Models.Organization>> GetOrganization(int id)
    {
        try
        {
            var org = await _db.QuerySingleOrDefaultAsync<Models.Organization>(
                "SELECT Id, Name, Description, Domain, OrgaPicAsBase64 FROM Organizations WHERE Id = @Id",
                new { Id = id });

            return org != null ? Result.Ok(org) : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<List<OrganizationMemberDto>>> GetMembersByDomain(string domain)
    {
        try
        {
            var sql = @"
            SELECT a.Email, u.Firstname, u.Lastname, om.Role
            FROM OrganizationMembers om
            JOIN Accounts a ON om.AccountId = a.Id
            JOIN Users u ON a.UserId = u.Id
            JOIN Organizations o ON om.OrganizationId = o.Id
            WHERE o.Domain = @Domain;";

            var result = await _db.QueryAsync<OrganizationMemberDto>(
                sql,
                new { Domain = domain });

            return Result.Ok(result.ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
    
    // === Schreiben ===

    public async Task<Result<Models.Organization>> CreateOrganization(
    string name,
    string description,
    string domain,
    string ownerFirstName,
    string ownerLastName,
    string ownerEmail,
    string initialPassword)
    {
        using var tx = _db.BeginSafeTransaction();

        try
        {
            // 1. Organisation einfügen
            var orgInsert = @"
            INSERT INTO Organizations (Name, Description, Domain)
            VALUES (@Name, @Description, @Domain);";

            var orgId = await _db.ExecuteScalarAsync<int>(
                orgInsert, new { Name = name, Description = description, Domain = domain }, tx);

            // 2. User einfügen
            var userInsert = @"
            INSERT INTO Users (Firstname, Lastname, Password)
            VALUES (@Firstname, @Lastname, @Password);";

            var userId = await _db.ExecuteScalarAsync<int>(
                $"{userInsert} SELECT last_insert_rowid();",
                new { Firstname = ownerFirstName, Lastname = ownerLastName, Password = initialPassword },
                tx);

            // 3. Account einfügen
            var accountInsert = @"
            INSERT INTO Accounts (Email, IsVerified, UserId)
            VALUES (@Email, 1, @UserId);";

            var accountId = await _db.ExecuteScalarAsync<int>(
                $"{accountInsert} SELECT last_insert_rowid();",
                new { Email = ownerEmail, UserId = userId },
                tx);

            // 4. Rollenbindung (Owner)
            var memberInsert = @"
            INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role)
            VALUES (@AccountId, @OrganizationId, @Role);";

            await _db.ExecuteAsync(memberInsert, new
            {
                AccountId = accountId,
                OrganizationId = orgId,
                Role = (int)Role.Owner
            }, tx);

            tx.Commit();

            return await GetOrganization(orgId);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> EditOrganization(Models.Organization organization)
    {
        try
        {
            var rows = await _db.ExecuteAsync(@"
                UPDATE Organizations
                SET Name = @Name,
                    Description = @Description,
                    Domain = @Domain,
                    OrgaPicAsBase64 = @OrgaPicAsBase64
                WHERE Id = @Id", new
            {
                organization.Name,
                organization.Description,
                organization.Domain,
                organization.OrgaPicAsBase64,
                organization.Id
            });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception)
        {
            return Result.Fail("DBError");
        }
    }

    public async Task<Result> UpdateOrganization(int id, OrganizationDto dto)
    {
        // 1. Objekt holen
        var orgResult = await GetOrganization(id);
        if (orgResult.IsFailed) return Result.Fail("Organisation nicht gefunden");
        var org = orgResult.Value;

        // 2. Domain-Eindeutigkeit prüfen
        if (!string.Equals(org.Domain, dto.Domain, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await _db.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM Organizations 
              WHERE Domain = @Domain AND Id <> @Id",
                new { dto.Domain, Id = id });

            if (duplicate > 0)
                return Result.Fail("Domain existiert bereits");
        }

        // 3. Felder mappen
        org.Name            = dto.Name;
        org.Description     = dto.Description;
        org.Domain          = dto.Domain;
        org.OrgaPicAsBase64 = dto.OrgaPicAsBase64;

        // 4. Persistieren
        return await EditOrganization(org);
    }

    // === Löschen ===

    public async Task<Result> DeleteOrganization(string domain)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM Organizations WHERE Domain = @Domain",
                new { Domain = domain });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteOrganization(int id)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM Organizations WHERE Id = @Id",
                new { Id = id });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception)
        {
            return Result.Fail("DBError");
        }
    }
}
