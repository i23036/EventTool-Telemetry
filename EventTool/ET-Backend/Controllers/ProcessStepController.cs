using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Stellt API-Endpunkte zur Verwaltung von Prozessschritten bereit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessStepController : ControllerBase
    {
        /// <summary>
        /// Ruft alle Prozessschritte oder Beispielwerte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Prozessschrittwerten.</returns>
        // GET: api/<ProcessStepController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft einen bestimmten Prozessschritt anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des Prozessschritts.</param>
        /// <returns>Der Prozessschrittwert als Zeichenkette.</returns>
        // GET api/<ProcessStepController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Erstellt einen neuen Prozessschritt.
        /// </summary>
        /// <param name="value">Die Daten des neuen Prozessschritts.</param>
        // POST api/<ProcessStepController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Aktualisiert einen bestehenden Prozessschritt anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Prozessschritts.</param>
        /// <param name="value">Die neuen Daten des Prozessschritts.</param>
        // PUT api/<ProcessStepController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht einen bestimmten Prozessschritt anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Prozessschritts.</param>
        // DELETE api/<ProcessStepController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
