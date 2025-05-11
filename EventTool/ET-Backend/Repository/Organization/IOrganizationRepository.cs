using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Organization;

/// <summary>
/// Definiert Methoden zum Zugriff auf Organisationsdaten in der Datenquelle.
/// </summary>
public interface IOrganizationRepository
{
    public Task<Result<bool>> OrganizationExists(String domain);
    public Task<Result<bool>> OrganizationExists(int id);
    public Task<Result<Models.Organization>> CreateOrganization(String name, String description, String domain);
    public Task<Result> DeleteOrganization(string domain);
    public Task<Result> DeleteOrganization(int id);
    public Task<Result<Models.Organization>> GetOrganization(String domain);
    public Task<Result<Models.Organization>> GetOrganization(int id);
    public Task<Result<List<Models.Organization>>> GetAllOrganizations();
    public Task<Result> EditOrganization(Models.Organization organization);
}