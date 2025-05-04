using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Bietet API-Endpunkte zur Verwaltung von Dateien.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FileManagementController : ControllerBase
    {
        /// <summary>
        /// Ruft alle verfügbaren Dateien oder Dateinamen ab.
        /// </summary>
        /// <returns>Eine Liste von Beispiel-Dateinamen.</returns>
        // GET: api/<FileManagementController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Ruft eine bestimmte Datei anhand der ID ab.
        /// </summary>
        /// <param name="id">Die ID der Datei.</param>
        /// <returns>Der Dateiname oder Dateipfad als Zeichenkette.</returns>
        // GET api/<FileManagementController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Legt eine neue Datei an oder speichert Informationen zur Datei.
        /// </summary>
        /// <param name="value">Dateibezogener Wert oder Inhalt als Zeichenkette.</param>
        // POST api/<FileManagementController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Aktualisiert eine vorhandene Datei anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu aktualisierenden Datei.</param>
        /// <param name="value">Der neue Wert oder Inhalt.</param>
        // PUT api/<FileManagementController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Löscht eine bestimmte Datei anhand der ID.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden Datei.</param>
        // DELETE api/<FileManagementController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
