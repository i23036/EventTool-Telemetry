using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Repository.Organization;

/// <summary>
/// Definiert Zugriffsmethoden auf Organisationsdaten in der Datenbank.
/// </summary>
public interface IOrganizationRepository
{
    // === Existenzprüfung ===

    Task<Result<bool>> OrganizationExists(string domain);
    Task<Result<bool>> OrganizationExists(int id);

    // === Lesen ===

    Task<Result<List<Models.Organization>>> GetAllOrganizations();
    Task<Result<Models.Organization>> GetOrganization(string domain);
    Task<Result<Models.Organization>> GetOrganization(int id);
    Task<Result<List<OrganizationMemberDto>>> GetMembersByDomain(string domain);


    // === Schreiben ===

    /// <summary>
    /// Erstellt eine neue Organisation und verknüpft direkt einen Owner.
    /// </summary>
    Task<Result<Models.Organization>> CreateOrganization(
        string name,
        string description,
        string domain,
        string ownerFirstName,
        string ownerLastName,
        string ownerEmail,
        string initialPassword
    );

    Task<Result> EditOrganization(Models.Organization organization);
    Task<Result> UpdateOrganization(int id, OrganizationDto dto);

    // === Löschen ===

    Task<Result> DeleteOrganization(string domain);
    Task<Result> DeleteOrganization(int id);
}