using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Stellt API-Endpunkte zur Verwaltung von Benutzern bereit.
    /// </summary>
   
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        /// <summary>
        /// Ruft alle Kontenwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispielwerten.</returns>

        // GET: api/<AccountController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft einen bestimmten Kontowert anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des gewünschten Kontowerts.</param>
        /// <returns>Der zugehörige Wert als Zeichenkette.</returns>

        // GET api/<AccountController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        /// <summary>
        /// Erstellt einen neuen Kontowert.
        /// </summary>
        /// <param name="value">Der zu speichernde Wert.</param>

        // POST api/<AccountController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }


        /// <summary>
        /// Aktualisiert einen bestehenden Kontowert anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Werts.</param>
        /// <param name="value">Der neue Wert.</param>

        // PUT api/<AccountController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht einen bestimmten Kontowert anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Werts.</param>

        // DELETE api/<AccountController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
