using ET.Shared.DTOs;
using ET_Backend.Repository.Person;
using ET_Backend.Services.Organization;
using FluentResults;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ET_Backend.Controllers;

/// <summary>
/// Bietet API-Endpunkte zur Verwaltung von Organisationen.
/// </summary>
[Route("api/organization")]
[ApiController]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly IAccountRepository _accountRepository;

    public OrganizationController(IOrganizationService organizationService, IAccountRepository accountRepository)
    {
        _organizationService = organizationService;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Ruft alle Organisationen ab.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrganizationDto>>> GetAllOrganizations()
    {
        var result = await _organizationService.GetAllOrganizations();

        if (result.IsFailed)
            return BadRequest(result.Errors);

        var dtoList = result.Value
            .Select(org => new OrganizationDto(
                org.Id,
                org.Name,
                org.Domain,
                org.Description,
                org.OrgaPicAsBase64,
                "", "", "", "")) // Ownerdaten nicht nötig bei GET
            .ToList();

        return Ok(dtoList);
    }

    /// <summary>
    /// Liefert die Organisation zur angegebenen Domain.
    /// </summary>
    [HttpGet("{domain}")]
    [Authorize]
    public async Task<ActionResult<OrganizationDto>> GetOrganizationByDomain(string domain)
    {
        var result = await _organizationService.GetOrganization(domain);

        if (result.IsFailed)
            return NotFound(result.Errors);

        var org = result.Value;
        return Ok(new OrganizationDto(
            org.Id,
            org.Name,
            org.Domain,
            org.Description,
            org.OrgaPicAsBase64,
            "", "", "", "" // Ownerdaten irrelevant bei GET
        ));
    }

    /// <summary>
    /// Gibt alle Mitglieder einer Organisation anhand der Domain zurück.
    /// </summary>
    [HttpGet("{domain}/members")]
    [Authorize]
    public async Task<IActionResult> GetMembersByDomain(string domain)
    {
        var result = await _organizationService.GetMembersByDomain(domain);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    /// <summary>
    /// Erstellt eine neue Organisation mit einem initialen Owner.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationDto value)
    {
        var result = await _organizationService.CreateOrganization(
            value.Name,
            value.Domain,
            value.Description,
            value.OwnerFirstName,
            value.OwnerLastName,
            value.OwnerEmail,
            value.InitialPassword
        );

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    /// <summary>
    /// Aktualisiert die Stammdaten einer Organisation.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] OrganizationDto dto)
    {
        // 🔍 Alte Domain abfragen
        var oldOrgResult = await _organizationService.GetOrganization(id);
        if (oldOrgResult.IsFailed)
            return BadRequest("Alte Organisation konnte nicht geladen werden.");

        var oldDomain = oldOrgResult.Value.Domain;

        // 🛠 Organisation aktualisieren
        var result = await _organizationService.UpdateOrganization(id, dto);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        // ✉️ Falls Domain geändert wurde → E-Mails anpassen
        var newDomain = dto.Domain;
        if (!string.Equals(oldDomain, newDomain, StringComparison.OrdinalIgnoreCase))
        {
            var updateEmails = await _accountRepository.UpdateEmailDomainsForOrganization(id, oldDomain, newDomain);
            if (updateEmails.IsFailed)
                return StatusCode(500, updateEmails.Errors);
        }

        return Ok();
    }

    /// <summary>
    /// Löscht eine Organisation anhand der Domain.
    /// </summary>
    [HttpDelete("{domain}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteOrganization(string domain)
    {
        var result = await _organizationService.DeleteOrganization(domain);

        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    // Rollen­änderung – neue Route, damit Role-Id im Pfad steckt
    [HttpPut("{domain}/members/{email}/role/{roleId:int}")]
    public async Task<IActionResult> UpdateRole(
        string domain,
        string email,
        int    roleId)                // Enum-Wert (int)
    {
        var result = await _organizationService.UpdateMemberRole(domain, email, roleId);
        return result.ToActionResult();   // FluentResultsExtension macht aus dem FluentResult direkt ein ActionResult 
    }

    // Mitglied entfernen (Domain + E-Mail)
    [HttpDelete("{domain}/members/{email}")]
    public async Task<IActionResult> RemoveMember(string domain, string email)
    {
        var result = await _organizationService.RemoveMember(domain, email);
        return result.ToActionResult();
    }
}
