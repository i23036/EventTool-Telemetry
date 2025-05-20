using ET_Backend.Repository.Organization;
using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Services.Organization;

/// <summary>
/// Implementierung des IOrganizationService für Organisationen.
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationService(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    // === Abfragen ===

    public async Task<Result<bool>> OrganizationExists(string domain) =>
        await _organizationRepository.OrganizationExists(domain);

    public async Task<Result<bool>> OrganizationExists(int id) =>
        await _organizationRepository.OrganizationExists(id);

    public async Task<Result<List<Models.Organization>>> GetAllOrganizations() =>
        await _organizationRepository.GetAllOrganizations();

    public async Task<Result<Models.Organization>> GetOrganization(string domain) =>
        await _organizationRepository.GetOrganization(domain);

    public async Task<Result<Models.Organization>> GetOrganization(int id) =>
        await _organizationRepository.GetOrganization(id);

    // === Erstellen & Bearbeiten ===

    public async Task<Result<Models.Organization>> CreateOrganization(string name, string domain, string description) =>
        await _organizationRepository.CreateOrganization(name, description, domain);

    public async Task<Result> EditOrganization(Models.Organization organization) =>
        await _organizationRepository.EditOrganization(organization);

    public async Task<Result> UpdateOrganization(string domain, OrganizationDto dto) =>
        await _organizationRepository.UpdateOrganization(domain, dto);

    // === Löschen ===

    public async Task<Result> DeleteOrganization(string domain) =>
        await _organizationRepository.DeleteOrganization(domain);

    public async Task<Result> DeleteOrganization(int id) =>
        await _organizationRepository.DeleteOrganization(id);
}
