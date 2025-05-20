using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Services.Organization;

/// <summary>
/// Service-Interface für alle Organisations-Operationen.
/// </summary>
public interface IOrganizationService
{
    // === Abfragen ===

    /// <summary>Prüft, ob eine Organisation anhand der Domain existiert.</summary>
    Task<Result<bool>> OrganizationExists(string domain);

    /// <summary>Prüft, ob eine Organisation anhand der ID existiert.</summary>
    Task<Result<bool>> OrganizationExists(int id);

    /// <summary>Gibt alle Organisationen im System zurück.</summary>
    Task<Result<List<Models.Organization>>> GetAllOrganizations();

    /// <summary>Gibt eine Organisation anhand der Domain zurück.</summary>
    Task<Result<Models.Organization>> GetOrganization(string domain);

    /// <summary>Gibt eine Organisation anhand der ID zurück.</summary>
    Task<Result<Models.Organization>> GetOrganization(int id);

    Task<Result<List<OrganizationMemberDto>>> GetMembersByDomain(string domain);



    // === Erstellen & Bearbeiten ===

    /// <summary>Erstellt eine neue Organisation.</summary>
    Task<Result<OrganizationDto>> CreateOrganization(
        string orgName,
        string domain,
        string description,
        string ownerFirstName,
        string ownerLastName,
        string ownerEmail,
        string initialPassword
    );

    /// <summary>Bearbeitet die interne Struktur einer Organisation (z. B. Admin-Tools).</summary>
    Task<Result> EditOrganization(Models.Organization organization);

    /// <summary>Aktualisiert eine Organisation mit DTO-Daten (z. B. aus dem Frontend).</summary>
    Task<Result> UpdateOrganization(string domain, OrganizationDto dto);

    // === Löschen ===

    /// <summary>Löscht eine Organisation anhand der Domain.</summary>
    Task<Result> DeleteOrganization(string domain);

    /// <summary>Löscht eine Organisation anhand der ID.</summary>
    Task<Result> DeleteOrganization(int id);
}