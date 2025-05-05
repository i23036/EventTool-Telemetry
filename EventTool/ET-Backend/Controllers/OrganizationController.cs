using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ET_Backend.Models;
using ET_Backend.Repository.Organization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Bietet API-Endpunkte zur Verwaltung von Organisationen.
    /// </summary>
    [Route("api/organizations")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationRepository _repo;

        public OrganizationController(IOrganizationRepository repo)
            => _repo = repo;

        /// <summary>
        /// Ruft alle Organisationen oder Beispielwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Organisationswerten.</returns>
        // GET api/v1/organizations
        [HttpGet]
        public async Task<IEnumerable<Organization>> GetAll()
            => await _repo.GetAllAsync();

        /// <summary>
        /// Ruft eine bestimmte Organisation anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID der Organisation.</param>
        /// <returns>Der Organisationswert als Zeichenkette.</returns>
        // GET api/v1/organizations/{domain}
        [HttpGet("{domain}")]
        public async Task<IActionResult> Get(string domain)
        {
            var org = await _repo.GetOrganization(domain);
            return org is null ? NotFound() : Ok(org);
        }

        /// <summary>
        /// Erstellt eine neue Organisation.
        /// </summary>
        /// <param name="value">Die Informationen zur Organisation.</param>
        // POST api/v1/organizations
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Post([FromBody] Organization org)
        {
            if (await _repo.OrganizationExists(org.Domain))
                return Conflict($"Domain '{org.Domain}' already exists.");

            var created = await _repo.CreateOrganization(org.Name, org.Description, org.Domain);
            return created ? CreatedAtAction(nameof(Get), new { domain = org.Domain }, org)
                : BadRequest("Could not create organization");
        }

        /// <summary>
        /// Aktualisiert eine vorhandene Organisation anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu aktualisierenden Organisation.</param>
        /// <param name="value">Die neuen Daten der Organisation.</param>
        // PUT api/<OrganizationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht eine bestimmte Organisation anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden Organisation.</param>
        // DELETE api/v1/organizations/{domain}
        [HttpDelete("{domain}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string domain)
        {
            var deleted = await _repo.DeleteOrganization(domain);
            return deleted ? NoContent() : NotFound();
        }
    }
}
