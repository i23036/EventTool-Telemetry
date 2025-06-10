using System.Data;
using System.Security.Claims;
using Dapper;
using ET_Backend.Repository.Authentication;
using ET.Shared.DTOs;
using ET_Backend.Services.Helper.Authentication;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ET_Backend.Controllers

{
    /// <summary>
    /// Bietet Endpunkte zur Authentifizierung von Benutzern.
    /// </summary>

    [Route("api/authenticate")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IAuthenticateService _authenticateService;
        private readonly ILogger<AuthenticateController> _logger;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="AuthenticateController"/>.
        /// </summary>
        /// <param name="authenticateService">Service zur Benutzer-Authentifizierung.</param>
        
        public AuthenticateController(IAuthenticateService authenticateService, ILogger<AuthenticateController> logger)
        {
            _authenticateService = authenticateService;
            _logger = logger;
        }

        /// <summary>
        /// Authentifiziert einen Benutzer anhand der übergebenen Anmeldedaten.
        /// </summary>
        /// <param name="value">Ein Objekt mit E-Mail und Passwort.</param>
        /// <returns>
        /// Gibt bei Erfolg ein Token zurück, andernfalls eine BadRequest-Antwort mit Fehlermeldung.
        /// </returns>
        /// <response code="200">Benutzer erfolgreich authentifiziert.</response>
        /// <response code="400">Authentifizierung fehlgeschlagen.</response>

        // POST api/<AuthenticateController>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto value)
        {
            Result<String> result = await _authenticateService.LoginUser(value.EMail, value.Password);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                var error = result.Errors.FirstOrDefault()?.Message ?? "Unbekannter Fehler";
                return BadRequest(new { error });
            }
        }

        /// <summary>
        /// Registriert einen neuen Benutzer und loggt ihn direkt ein.
        /// </summary>
        /// <param name="dto">Ein Objekt mit Vorname, Nachname, E-Mail und Passwort.</param>
        /// <returns>
        /// Gibt bei Erfolg das JWT-Token des neu registrierten Benutzers zurück,
        /// andernfalls eine Problem-Antwort mit Fehlerbeschreibung.
        /// </returns>
        /// <response code="200">Benutzer erfolgreich registriert und eingeloggt.</response>
        /// <response code="400">Registrierung fehlgeschlagen – z. B. Benutzer existiert bereits.</response>
        
        // POST api/<AuthenticateController>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authenticateService.RegisterUser(
                dto.FirstName, 
                dto.LastName, 
                dto.EMail, 
                dto.Password
                );

            if (result.IsSuccess)
            {
                return Ok(new { message = result.Value });
            }
            var error = result.Errors.FirstOrDefault()?.Message ?? "Unknown error";
            return BadRequest(new { error });
        }

        /// <summary>Wechselt die aktive Mitgliedschaft des angemeldeten Users.</summary>
        /// <remarks>Liefert ein frisches JWT mit Domain & Account des Ziel-Accounts.</remarks>
        [HttpPost("switch/{accountId:int}")]
        [Authorize]
        public async Task<IActionResult> SwitchAccount(int accountId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _authenticateService.SwitchAccount(accountId, currentUserId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Accountwechsel erfolgreich. Neuer AccountId: {AccountId}", accountId);
                return Ok(new { token = result.Value });
            }

            var err = result.Errors.First().Message;
            _logger.LogWarning("Fehler beim Accountwechsel: {Error}", err);

            // Keine Forbid mehr, sondern immer 400 für bessere API-Feedbacks
            return BadRequest(new { error = err });
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(
            [FromQuery] string token,
            [FromServices] IEmailVerificationTokenRepository repo,
            [FromServices] IDbConnection db,
            [FromServices] IOptions<JwtOptions> jwt,
            [FromServices] ILogger<AuthenticateController> log)
        {
            var result = await repo.GetAsync(token);
            if (result.IsFailed || result.Value.ExpiresAt < DateTime.UtcNow)
            {
                log.LogWarning("Ungültiger oder abgelaufener Token: {Token}", token);
                return Redirect($"{jwt.Value.FrontendBaseUrl}verification?status=invalid");
            }

            var accId = result.Value.AccountId;

            if (db.State != ConnectionState.Open)
                db.Open();

            using var tr = db.BeginTransaction();
            await db.ExecuteAsync("UPDATE Accounts SET IsVerified = 1 WHERE Id = @id", new { id = accId }, tr);

            log.LogWarning("Versuche Token zu löschen: {Token}", token);
            var del = await repo.ConsumeAsync(token, db, tr);
            if (del.IsFailed)
            {
                tr.Rollback();
                log.LogError("Token-DELETE fehlgeschlagen: {Error}", del.Errors.FirstOrDefault()?.Message);
                return StatusCode(500, "Token konnte nicht gelöscht werden.");
            }

            tr.Commit();
            log.LogInformation("Account {Id} wurde erfolgreich verifiziert", accId);

            return Redirect($"{jwt.Value.FrontendBaseUrl}login?verified=true");
        }
    }
}