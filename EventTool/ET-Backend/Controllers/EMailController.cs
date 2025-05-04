using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Stellt API-Endpunkte zur Verwaltung von E-Mail-bezogenen Daten bereit.
    /// </summary>
    
    [Route("api/[controller]")]
    [ApiController]
    public class EMailController : ControllerBase
    {
        // GET: api/<EMailController>
        /// <summary>
        /// Ruft alle E-Mail-Werte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispielwerten.</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<EMailController>/5
        /// <summary>
        /// Ruft einen bestimmten E-Mail-Wert anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des gewünschten Werts.</param>
        /// <returns>Der E-Mail-Wert als Zeichenkette.</returns>
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<EMailController>
        /// <summary>
        /// Erstellt einen neuen E-Mail-Wert.
        /// </summary>
        /// <param name="value">Der zu speichernde Wert.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<EMailController>/5
        /// <summary>
        /// Aktualisiert einen bestehenden E-Mail-Wert anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Werts.</param>
        /// <param name="value">Der neue Wert.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EMailController>/5
        /// <summary>
        /// Löscht einen bestimmten E-Mail-Wert anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Werts.</param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
