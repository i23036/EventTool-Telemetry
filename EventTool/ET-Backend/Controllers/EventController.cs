using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Bietet API-Endpunkte zur Verwaltung von Event-Daten.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        /// <summary>
        /// Ruft alle Event-Werte ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Events.</returns>
        // GET: api/<EventController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft ein bestimmtes Event anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID des Events.</param>
        /// <returns>Das Event als Zeichenkette.</returns>
        // GET api/<EventController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Erstellt ein neues Event.
        /// </summary>
        /// <param name="value">Der Event-Wert als Zeichenkette.</param>
        // POST api/<EventController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Aktualisiert ein bestehendes Event anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu aktualisierenden Events.</param>
        /// <param name="value">Der neue Wert.</param>
        // PUT api/<EventController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht ein bestimmtes Event anhand der ID.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Events.</param>
        // DELETE api/<EventController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
