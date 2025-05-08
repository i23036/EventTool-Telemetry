using ET_Backend.Repository.Organization;
using FluentResults;

namespace ET_Backend.Services.Organization;

public class OrganizationService : IOrganizationService
{
    private IOrganizationRepository _organizationRepository;

    public OrganizationService(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<bool>> OrganizationExists(String domain)
    {
        return await _organizationRepository.OrganizationExists(domain);
    }

    public async Task<Result> CreateOrganization(String name, String domain, String description)
    {
        return await _organizationRepository.CreateOrganization(name, description, domain);
    }

    public async Task<Result> DeleteOrganization(String domain)
    {
        return await _organizationRepository.DeleteOrganization(domain);
    }

    public async Task<Result<List<Models.Organization>>> GetAllOrganizations()
    {
        return await _organizationRepository.GetAllOrganizations();
    }
}