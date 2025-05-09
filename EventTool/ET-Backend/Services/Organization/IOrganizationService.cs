using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Services.Organization;

public interface IOrganizationService
{
    public Task<Result<bool>> OrganizationExists(String domain);

    public Task<Result> CreateOrganization(String name, String domain, String description);

    public Task<Result> DeleteOrganization(String  domain);

    public Task<Result<List<Models.Organization>>> GetAllOrganizations();
}