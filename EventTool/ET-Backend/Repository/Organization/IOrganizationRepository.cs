using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Organization;

/// <summary>
/// Definiert Methoden zum Zugriff auf Organisationsdaten in der Datenquelle.
/// </summary>
public interface IOrganizationRepository
{
    /// <summary>
    /// Prüft, ob eine Organisation mit der angegebenen Domain existiert.
    /// </summary>
    /// <param name="domain">Die Domain der Organisation.</param>
    /// <returns>Ein Task mit true, wenn die Organisation existiert.</returns>
    public Task<Result<bool>> OrganizationExists(String domain);

    /// <summary>
    /// Erstellt eine neue Organisation mit Name, Beschreibung und Domain.
    /// </summary>
    /// <param name="name">Der Name der Organisation.</param>
    /// <param name="description">Die Beschreibung der Organisation.</param>
    /// <param name="domain">Die Domain der Organisation.</param>
    /// <returns>Ein Task mit true, wenn die Organisation erfolgreich erstellt wurde.</returns>
    public Task<Result> CreateOrganization(String name, String description, String domain);

    /// <summary>
    /// Fügt einer bestehenden Organisation ein Benutzerkonto hinzu.
    /// </summary>
    /// <param name="organization">Die ID der Organisation.</param>
    /// <param name="account">Das hinzuzufügende Benutzerkonto.</param>
    /// <returns>Ein Task mit true, wenn das Konto erfolgreich hinzugefügt wurde.</returns>
    public Task<Result> AddAccount(int organization, Account account);

    /// <summary>
    /// Ruft eine Organisation anhand ihrer Domain ab.
    /// </summary>
    /// <param name="domain">Die Domain der Organisation.</param>
    /// <returns>Ein Task mit dem <see cref="Models.Organization"/>-Objekt.</returns>
    public Task<Result<Models.Organization>> GetOrganization(String domain);

    /// <summary>
    /// Ruft alle Organisationen aus der Datenbank ab.
    /// </summary>
    public Task<Result<List<Models.Organization>>> GetAllOrganizations();

    /// <summary>
    /// Löscht die Organisation mit der angegebenen Domain.
    /// </summary>
    public Task<Result> DeleteOrganization(string domain);

}