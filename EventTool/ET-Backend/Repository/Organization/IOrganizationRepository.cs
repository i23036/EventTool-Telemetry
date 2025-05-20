using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Repository.Organization;

/// <summary>
/// Definiert Zugriffsmethoden auf Organisationsdaten in der Datenbank.
/// </summary>
public interface IOrganizationRepository
{
    // === Existenzprüfung ===

    /// <summary>Prüft, ob eine Organisation mit gegebener Domain existiert.</summary>
    Task<Result<bool>> OrganizationExists(string domain);

    /// <summary>Prüft, ob eine Organisation mit gegebener ID existiert.</summary>
    Task<Result<bool>> OrganizationExists(int id);

    // === Lesen ===

    /// <summary>Lädt alle Organisationen.</summary>
    Task<Result<List<Models.Organization>>> GetAllOrganizations();

    /// <summary>Lädt eine Organisation anhand der Domain.</summary>
    Task<Result<Models.Organization>> GetOrganization(string domain);

    /// <summary>Lädt eine Organisation anhand der ID.</summary>
    Task<Result<Models.Organization>> GetOrganization(int id);

    // === Schreiben ===

    /// <summary>Erstellt eine neue Organisation.</summary>
    Task<Result<Models.Organization>> CreateOrganization(string name, string description, string domain);

    /// <summary>Bearbeitet eine Organisation direkt (interner Aufruf).</summary>
    Task<Result> EditOrganization(Models.Organization organization);

    /// <summary>Aktualisiert eine Organisation per DTO (Frontend).</summary>
    Task<Result> UpdateOrganization(string domain, OrganizationDto dto);

    // === Löschen ===

    /// <summary>Löscht eine Organisation anhand der Domain.</summary>
    Task<Result> DeleteOrganization(string domain);

    /// <summary>Löscht eine Organisation anhand der ID.</summary>
    Task<Result> DeleteOrganization(int id);
}