using ET_Backend.Models;

namespace ET_Backend.Repository.Organization;

public class OrganizationRepository : IOrganizationRepository
{
    public Task<bool> OrganizationExists(String domain)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateOrganization(String name, String description, String domain)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddAccount(int organization, Account account)
    {
        throw new NotImplementedException();
    }

    public Task<Models.Organization> GetOrganization(String domain)
    {
        throw new NotImplementedException();
    }
}