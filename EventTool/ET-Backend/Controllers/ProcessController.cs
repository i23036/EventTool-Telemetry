using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Stellt API-Endpunkte zur Verwaltung von Prozessen bereit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        /// <summary>
        /// Ruft alle Prozesse oder Beispielwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Prozesswerten.</returns>
        // GET: api/<ProcessController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft einen bestimmten Prozess anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des Prozesses.</param>
        /// <returns>Der Prozesswert als Zeichenkette.</returns>
        // GET api/<ProcessController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Erstellt einen neuen Prozess.
        /// </summary>
        /// <param name="value">Die Daten des neuen Prozesses.</param>
        // POST api/<ProcessController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Aktualisiert einen bestehenden Prozess anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Prozesses.</param>
        /// <param name="value">Die neuen Daten des Prozesses.</param>
        // PUT api/<ProcessController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht einen bestimmten Prozess anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Prozesses.</param>
        // DELETE api/<ProcessController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
