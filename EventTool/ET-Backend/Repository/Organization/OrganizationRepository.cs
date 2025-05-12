using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Organization;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly IDbConnection _db;

    public OrganizationRepository(IDbConnection db)
    {
        _db = db;
    }

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

    public async Task<Result<Models.Organization>> GetOrganization(string domain)
    {
        try
        {
            var org = await _db.QuerySingleOrDefaultAsync<Models.Organization>(
                "SELECT Id, Name, Description, Domain FROM Organizations WHERE Domain = @Domain",
                new { Domain = domain });

            if (org == null)
                return Result.Fail("NotFound");

            return Result.Ok(org);
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
                "SELECT Id, Name, Description, Domain FROM Organizations WHERE Id = @Id",
                new { Id = id });

            if (org == null)
                return Result.Fail("NotFound");

            return Result.Ok(org);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<List<Models.Organization>>> GetAllOrganizations()
    {
        try
        {
            var orgs = (await _db.QueryAsync<Models.Organization>(
                "SELECT Id, Name, Description, Domain FROM Organizations")).ToList();

            return Result.Ok(orgs);
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
                    Domain = @Domain
                WHERE Id = @Id",
                new
                {
                    organization.Name,
                    organization.Description,
                    organization.Domain,
                    organization.Id
                });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception)
        {
            return Result.Fail("DBError");
        }
    }
}
