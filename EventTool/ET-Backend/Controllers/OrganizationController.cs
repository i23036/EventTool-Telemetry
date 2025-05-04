using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Bietet API-Endpunkte zur Verwaltung von Organisationen.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        /// <summary>
        /// Ruft alle Organisationen oder Beispielwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Organisationswerten.</returns>
        // GET: api/<OrganizationController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft eine bestimmte Organisation anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID der Organisation.</param>
        /// <returns>Der Organisationswert als Zeichenkette.</returns>
        // GET api/<OrganizationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Erstellt eine neue Organisation.
        /// </summary>
        /// <param name="value">Die Informationen zur Organisation.</param>
        // POST api/<OrganizationController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
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
        // DELETE api/<OrganizationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
