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
            const string sql = @"
              SELECT COUNT(1)
                FROM Organizations
               WHERE Domain = @Domain;
            ";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { Domain = domain });
            return Result.Ok<bool>(count > 0);
        }
        catch (Exception e)
        {
            return Result.Fail<bool>("DBError");
        }
    }

    public async Task<Result> CreateOrganization(string name, string description, string domain)
    {
        try
        {
            const string sql = @"
              INSERT INTO Organizations (Name, Description, Domain)
              VALUES (@Name, @Description, @Domain);
            ";
            await _db.ExecuteAsync(sql, new { Name = name, Description = description, Domain = domain });
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }

    }


    public async Task<Result> AddAccount(int organization, Account account)
    {
        try
        {
            throw new NotImplementedException();
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }

    public async Task<Result<Models.Organization>> GetOrganization(string domain)
    {
        try
        {
            const string sql = @"
              SELECT Name,
                     Description,
                     Domain
                FROM Organizations
               WHERE Domain = @Domain;
            ";
            return Result.Ok(await _db.QuerySingleOrDefaultAsync<Models.Organization>(sql, new { Domain = domain }));
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }

    public async Task<Result<List<Models.Organization>>> GetAllOrganizations()
    {
        try
        {
            const string sql = @"
              SELECT Name,
                     Description,
                     Domain
                FROM Organizations;
            ";
            var result = await _db.QueryAsync<Models.Organization>(sql);
            return Result.Ok(result.ToList());
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }

    public async Task<Result> DeleteOrganization(string domain)
    {
        try
        {
            const string sql = @"
              DELETE
                FROM Organizations
               WHERE Domain = @Domain;
            ";
            await _db.ExecuteAsync(sql, new { Domain = domain });
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("DBError");
        }
    }
}