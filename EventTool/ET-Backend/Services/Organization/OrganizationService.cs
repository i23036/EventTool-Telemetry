using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using ET.Shared.DTOs;
using FluentResults;
using ET_Backend.Models.Enums;

namespace ET_Backend.Services.Organization;

/// <summary>
/// Implementierung des IOrganizationService für Organisationen.
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAccountRepository _accountRepository;


    public OrganizationService(IOrganizationRepository organizationRepository, IAccountRepository accountRepository)
    {
        _organizationRepository = organizationRepository;
        _accountRepository = accountRepository;
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

    public async Task<Result<List<OrganizationMemberDto>>> GetMembersByDomain(string domain)
        => await _organizationRepository.GetMembersByDomain(domain);


    // === Erstellen & Bearbeiten ===

    public async Task<Result<OrganizationDto>> CreateOrganization(
        string orgName,
        string domain,
        string description,
        string ownerFirstName,
        string ownerLastName,
        string ownerEmail,
        string initialPassword)
    {
        var result = await _organizationRepository.CreateOrganization(
            orgName, domain, description,
            ownerFirstName, ownerLastName, ownerEmail, initialPassword);

        if (result.IsFailed)
            return Result.Fail(result.Errors);

        var org = result.Value;

        // Mapping zum DTO
        var dto = new OrganizationDto(
            org.Name,
            org.Domain,
            org.Description,
            org.OrgaPicAsBase64,
            ownerFirstName,
            ownerLastName,
            ownerEmail,
            initialPassword // nur intern → nicht an UI zurückgeben
        );

        return Result.Ok(dto);
    }

    
    public async Task<Result> EditOrganization(Models.Organization organization) =>
        await _organizationRepository.EditOrganization(organization);

    public async Task<Result> UpdateOrganization(string domain, OrganizationDto dto) =>
        await _organizationRepository.UpdateOrganization(domain, dto);

    // === Löschen ===

    public async Task<Result> DeleteOrganization(string domain) =>
        await _organizationRepository.DeleteOrganization(domain);

    public async Task<Result> DeleteOrganization(int id) =>
        await _organizationRepository.DeleteOrganization(id);

    public async Task<Result> UpdateMemberRole(string domain, string email, int newRole)
    {
        // 1) Organisation holen
        var orgResult = await _organizationRepository.GetOrganization(domain);
        if (orgResult.IsFailed) return Result.Fail(orgResult.Errors);

        // 2) Account holen
        var accResult = await _accountRepository.GetAccount(email);
        if (accResult.IsFailed) return Result.Fail(accResult.Errors);

        var account = accResult.Value;

        // 3) Gehört der Account wirklich zu dieser Domain?
        if (!string.Equals(account.Organization?.Domain, domain, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("Benutzer gehört nicht zu dieser Organisation.");

        // 4) Rolle setzen + speichern
        account.Role = (Role)newRole;
        var upd = await _accountRepository.EditAccount(account);
        if (upd.IsFailed) return Result.Fail(upd.Errors);

        Console.WriteLine($"[ROLE UPDATED] {email} → {(Role)newRole} in '{domain}'");
        return Result.Ok();
    }

    public async Task<Result> RemoveMember(string domain, string email)
    {
        // 1) Org + Account holen
        var orgResult = await _organizationRepository.GetOrganization(domain);
        if (orgResult.IsFailed) return Result.Fail(orgResult.Errors);

        var accResult = await _accountRepository.GetAccount(email);
        if (accResult.IsFailed) return Result.Fail(accResult.Errors);

        var account = accResult.Value;

        // 2) Domain-Check (robust falls OrgId bereits null)
        if (!string.Equals(account.Organization?.Domain, domain, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("Benutzer gehört nicht (mehr) zu dieser Organisation.");

        // 3) Entfernen
        return await _accountRepository.RemoveFromOrganization(account.Id, orgResult.Value.Id);
    }
}
