using System.Data;
using Dapper;
using ET.Shared.DTOs;
using FluentResults;

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

    // === Schreiben ===

    public async Task<Result<Models.Organization>> CreateOrganization(string name, string description, string domain)
    {
        try
        {
            var sql = @"
                INSERT INTO Organizations (Name, Description, Domain)
                VALUES (@Name, @Description, @Domain);
                SELECT last_insert_rowid();";

            var id = await _db.ExecuteScalarAsync<int>(sql, new
            {
                Name = name,
                Description = description,
                Domain = domain
            });

            return await GetOrganization(id);
        }
        catch (Exception ex)
        {
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

    public async Task<Result> UpdateOrganization(string domain, OrganizationDto dto)
    {
        var orgResult = await GetOrganization(domain);
        if (orgResult.IsFailed) return Result.Fail("Organization not found");

        var org = orgResult.Value;
        org.Name = dto.Name;
        org.Description = dto.Description;
        org.Domain = dto.Domain;
        org.OrgaPicAsBase64 = dto.OrgaPicAsBase64;

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
