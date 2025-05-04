using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Stellt API-Endpunkte zur Verwaltung von Benutzerdaten bereit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Ruft alle Benutzer oder Beispielwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Benutzerdaten.</returns>
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft einen bestimmten Benutzer anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des Benutzers.</param>
        /// <returns>Die Benutzerdaten als Zeichenkette.</returns>
        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Erstellt einen neuen Benutzer.
        /// </summary>
        /// <param name="value">Die Daten des neuen Benutzers.</param>
        // POST api/<UserController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Aktualisiert einen bestehenden Benutzer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Benutzers.</param>
        /// <param name="value">Die neuen Benutzerdaten.</param>
        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht einen bestimmten Benutzer anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Benutzers.</param>
        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
