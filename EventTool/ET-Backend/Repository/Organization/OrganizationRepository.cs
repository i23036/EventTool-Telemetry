using System.Data;
using Dapper;
using ET_Backend.Models;

namespace ET_Backend.Repository.Organization;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly IDbConnection _db;

    public OrganizationRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<bool> OrganizationExists(string domain)
    {
        const string sql = @"
              SELECT COUNT(1)
                FROM Organizations
               WHERE Domain = @Domain;
            ";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { Domain = domain });
        return count > 0;
    }

    public async Task<bool> CreateOrganization(string name, string description, string domain)
    {
        const string sql = @"
              INSERT INTO Organizations (Name, Description, Domain)
              VALUES (@Name, @Description, @Domain);
            ";
        var affected = await _db.ExecuteAsync(sql, new { Name = name, Description = description, Domain = domain });
        return affected == 1;
    }

    public Task<bool> AddAccount(int organization, Account account)
    {
        // hier würdet ihr z.B. ein UPDATE auf Accounts durchführen:
        // UPDATE Accounts SET Organization = @Organization WHERE Email = @EMail
        throw new NotImplementedException();
    }

    public async Task<Models.Organization> GetOrganization(string domain)
    {
        const string sql = @"
              SELECT Name,
                     Description,
                     Domain
                FROM Organizations
               WHERE Domain = @Domain;
            ";
        return await _db.QuerySingleOrDefaultAsync<Models.Organization>(sql, new { Domain = domain });
    }

    public async Task<IEnumerable<Models.Organization>> GetAllAsync()
    {
        const string sql = @"
              SELECT Name,
                     Description,
                     Domain
                FROM Organizations;
            ";
        return await _db.QueryAsync<Models.Organization>(sql);
    }

    public async Task<bool> DeleteOrganization(string domain)
    {
        const string sql = @"
              DELETE
                FROM Organizations
               WHERE Domain = @Domain;
            ";
        var affected = await _db.ExecuteAsync(sql, new { Domain = domain });
        return affected > 0;
    }
}