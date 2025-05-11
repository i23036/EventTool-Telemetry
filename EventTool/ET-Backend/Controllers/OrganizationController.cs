using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ET_Backend.Models;
using ET_Backend.Repository.Organization;
using ET_Backend.Services.Organization;
using ET.Shared.DTOs;
using FluentResults;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Bietet API-Endpunkte zur Verwaltung von Organisationen.
    /// </summary>
    [Route("api/organization")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }


        /// <summary>
        /// Ruft alle Organisationen oder Beispielwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Organisationswerten.</returns>
        // GET api/v1/organizations
        [HttpGet]
        public async Task<ActionResult<List<OrganizationDto>>> GetAllOrganizations()
        {
            Result<List<Organization>> result = await _organizationService.GetAllOrganizations();

            if (result.IsSuccess)
            {
                List<OrganizationDto> dtoList = result.Value
                    .Select(org => new OrganizationDto(org.Name, org.Domain, org.Description))
                    .ToList();

                return Ok(dtoList);
            }
            else
            {
                return BadRequest(result.Value);
            }
        }


        /// <summary>
        /// Erstellt eine neue Organisation.
        /// </summary>
        /// <param name="value">Die Informationen zur Organisation.</param>
        // POST api/v1/organizations
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateOrganization([FromBody] OrganizationDto value)
        {
            Result<Organization> result = await _organizationService.CreateOrganization(value.Name, value.Domain, value.Description);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// Löscht eine bestimmte Organisation anhand der ID.
        /// </summary>
        /// <param name="domain">Die ID der zu löschenden Organisation.</param>
        // DELETE api/v1/organizations/{domain}
        [HttpDelete("{domain}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteOrganization(string domain)
        {
            Result result = await _organizationService.DeleteOrganization(domain);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
