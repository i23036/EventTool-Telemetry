using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ET_Backend.Services.Organization;
using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Controllers;

/// <summary>
/// Bietet API-Endpunkte zur Verwaltung von Organisationen.
/// </summary>
[Route("api/organization")]
[ApiController]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
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
            org.Name,
            org.Domain,
            org.Description,
            org.OrgaPicAsBase64,
            "", "", "", "" // Ownerdaten irrelevant bei GET
        ));
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
    /// Aktualisiert eine bestehende Organisation anhand der Domain.
    /// </summary>
    [HttpPut("{domain}")]
    [Authorize]
    public async Task<IActionResult> UpdateOrganization(string domain, [FromBody] OrganizationDto dto)
    {
        var result = await _organizationService.UpdateOrganization(domain, dto);

        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
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
}
