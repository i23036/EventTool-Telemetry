using ET_Backend.Models;

namespace ET_Backend.Repository.Organization;

public interface IOrganizationRepository
{
    public Task<bool> OrganizationExists(String domain);

    public Task<bool> CreateOrganization(String name, String description, String domain);

    public Task<bool> AddAccount(int organization, Account account);

    public Task<Models.Organization> GetOrganization(String domain);
}