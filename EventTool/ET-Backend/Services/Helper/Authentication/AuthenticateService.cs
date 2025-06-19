using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ET_Backend.Models;
using ET_Backend.Models.Enums;
using ET_Backend.Repository.Authentication;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using FluentResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ET_Backend.Services.Helper.Authentication;
/// <summary>
/// Service zur Authentifizierung von Benutzern und Generierung von JWTs.
/// </summary>
public class AuthenticateService : IAuthenticateService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthenticateService> _logger;
    private readonly IEmailVerificationTokenRepository _tokenRepo;
    private readonly IEMailService _emailService;


    /// <summary>
    /// Erstellt eine neue Instanz des <see cref="AuthenticateService"/>.
    /// </summary>
    /// <param name="accountRepository">Das Repository für Benutzerkonten.</param>
    /// <param name="userRepository"></param>
    /// <param name="organizationRepository"></param>
    /// <param name="jwtOptions">Konfiguration für das JWT-Token.</param>
    /// <param name="logger"></param>
    /// <param name="tokenRepo"></param>
    /// <param name="emailService"></param>
    public AuthenticateService(
        IAccountRepository accountRepository,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthenticateService> logger,
        IEmailVerificationTokenRepository tokenRepo,
        IEMailService emailService)
    {
        _accountRepository = accountRepository;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _tokenRepo = tokenRepo;
        _emailService = emailService; ;
    }

    /// <summary>
    /// Authentifiziert einen Benutzer anhand von E-Mail und Passwort.
    /// Gibt bei Erfolg ein JWT zurück.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Benutzers.</param>
    /// <param name="password">Das eingegebene Passwort.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem JWT bei Erfolg,
    /// oder einer lokalisierten Fehlermeldung.
    /// </returns>
    public async Task<Result<string>> LoginUser(string eMail, string password)
    {
        try
        {
            // Existenz prüfen
            var accountExists = await _accountRepository.AccountExists(eMail);
            if (accountExists.IsFailed)
            {
                return Result.Fail("Fehler beim Überprüfen des Benutzerkontos.");
            }

            if (!accountExists.Value)
                return Result.Fail("Es existiert kein Benutzer mit dieser E-Mail-Adresse.");

            // Account abrufen
            var accountResult = await _accountRepository.GetAccount(eMail);
            if (accountResult.IsFailed)
            {
                return Result.Fail("Benutzerdaten konnten nicht geladen werden.");
            }

            var account = accountResult.Value;

            // Verifizierungsstatus prüfen
            if (!account.IsVerified)
            {
                return Result.Fail("Account ist noch nicht bestätigt. Bitte E-Mail prüfen.");
            }

            // Passwort prüfen
            if (account.User.Password != password)
            {
                return Result.Fail("Das eingegebene Passwort ist falsch.");
            }

            // Token generieren und zurückgeben
            var token = GenerateJwtToken(account);
            return Result.Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Benutzeranmeldung.");
            return Result.Fail("Unerwarteter Fehler beim Login.");
        }
    }


    /// <summary>
    /// Registriert einen neuen Benutzer mit zugehörigem Benutzerkonto, wenn eine passende Organisation existiert.
    /// </summary>
    /// <param name="firstname">Der Vorname des Benutzers.</param>
    /// <param name="lastname">Der Nachname des Benutzers.</param>
    /// <param name="eMail">Die E-Mail-Adresse des Benutzers.</param>
    /// <param name="password">Das Passwort des Benutzers (im Klartext, wird intern verarbeitet).</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit einer Erfolgs- oder Fehlermeldung als Zeichenkette.
    /// <list type="bullet">
    /// <item><description><c>Ok("User added successfully")</c> – wenn alles funktioniert hat</description></item>
    /// <item><description><c>Fail("User already exists")</c> – wenn die E-Mail bereits registriert ist</description></item>
    /// <item><description><c>Fail("No organization exists for this E-Mail")</c> – wenn keine passende Organisation existiert</description></item>
    /// <item><description><c>Fail("Database failure")</c> – wenn ein Eintrag in DB fehlschlägt</description></item>
    /// </list>
    /// </returns>
    public async Task<Result<string>> RegisterUser(string firstname, string lastname, string eMail, string password)
    {
        try
        {
            // Existenz prüfen
            var accountExists = await _accountRepository.AccountExists(eMail);
            if (accountExists.IsFailed)
            {
                return Result.Fail("Fehler beim Überprüfen, ob das Benutzerkonto bereits existiert.");

                // TEMPORÄR: DEBUG: Fehlerausgabe
                //var dbError = accountExists.Errors[0].Message;
                //return Result.Fail<string>($"[DEBUG] {dbError}");
            }

            if (accountExists.Value)
                return Result.Fail("Ein Benutzer mit dieser E-Mail-Adresse existiert bereits.");

            // Domain extrahieren und Organisation prüfen
            var domain = eMail.Substring(eMail.LastIndexOf('@') + 1);
            var orgExists = await _organizationRepository.OrganizationExists(domain);

            if (orgExists.IsFailed)
                return Result.Fail("Fehler beim Überprüfen der Organisation zur E-Mail-Domain.");
            if (!orgExists.Value)
                return Result.Fail("Es existiert keine Organisation für diese E-Mail-Domain.");

            // Organisation laden
            var orgResult = await _organizationRepository.GetOrganization(domain);
            if (orgResult.IsFailed)
                return Result.Fail("Fehler beim Abrufen der Organisation.");

            // Neuen User vorbereiten
            var user = new User
            {
                Firstname = firstname,
                Lastname = lastname,
                Password = password
            };

            // Account anlegen
            var accountResult = await _accountRepository.CreateAccount(
                eMail,
                orgResult.Value,
                Role.Member,
                user);

            if (accountResult.IsFailed)
            {
                return Result.Fail("Fehler beim Anlegen des Benutzerkontos.");

                // TEMPORÄR: DEBUG: Fehlerausgabe
                //var dbError = accountResult.Errors[0].Message;
                //return Result.Fail<string>($"[DEBUG] {dbError}");
            }

            var accountId = accountResult.Value.Id;

            _logger.LogInformation("Registrierung erfolgreich, neue Account-ID: {Id}", accountId);

            var token = Guid.NewGuid().ToString("N");

            // Token speichern
            var tokenResult = await _tokenRepo.CreateAsync(accountId, token);
            if (tokenResult.IsFailed)
                return Result.Fail("Token konnte nicht gespeichert werden.");

            // Mail-Text vorbereiten
            var link = $"{_jwtOptions.BackendBaseUrl}api/authenticate/verify?token={token}";
            var body = $"""
                        <p>Hallo {firstname},</p>
                        <p>bitte bestätige deine Registrierung über folgenden Link:</p>
                        <p><a href="{link}">Account bestätigen</a></p>
                        <p>Nach der Bestätigung kannst du dich anmelden.</p>
                        <p><small>Der Link ist 48 Stunden gültig.</small></p>
                        """;

            // Mail senden
            await _emailService.SendAsync(eMail, "Bitte Registrierung bestätigen", body);

            // Rückmeldung
            return Result.Ok("Benutzer wurde erfolgreich registriert. Bitte E-Mail bestätigen.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Unerwarteter Fehler: {ex.Message}");
        }
    }

    public async Task<Result<string>> SwitchAccount(int accountId, int currentUserId)
    {
        var accResult = await _accountRepository.GetAccount(accountId);
        if (accResult.IsFailed)
            return Result.Fail("Account existiert nicht.");

        var acc = accResult.Value;

        if (acc.UserId != currentUserId)
        {
            _logger.LogWarning("AccountId {0} gehört nicht zu UserId {1}.", accountId, currentUserId);
            return Result.Fail("Mitgliedschaft konnte nicht geladen werden.");
        }

        // privater Helper bleibt privat
        var token = GenerateJwtToken(acc);
        return Result.Ok(token);
    }

    public async Task<Result> AddMembership(int userId, string newEmail)
    {
        // 1. Prüfen, ob E-Mail bereits existiert
        var exists = await _accountRepository.AccountExists(newEmail);
        if (exists.IsFailed || exists.Value)
            return Result.Fail("Ein Account mit dieser E-Mail existiert bereits.");

        // 2. Domain extrahieren und Orga laden
        var domain = newEmail.Substring(newEmail.LastIndexOf('@') + 1);
        var orgResult = await _organizationRepository.GetOrganization(domain);
        if (orgResult.IsFailed)
            return Result.Fail("Es existiert keine Organisation mit dieser Domain.");

        var organization = orgResult.Value;

        // 3. User laden
        var userResult = await _userRepository.GetUser(userId);
        if (userResult.IsFailed)
            return Result.Fail("Benutzer nicht gefunden.");

        var user = userResult.Value;

        // 4. Account + Mitgliedschaft anlegen
        var createResult = await _accountRepository.CreateAccount(
            newEmail,
            organization,
            Role.Member,
            user
        );

        if (createResult.IsFailed)
            return Result.Fail("Account konnte nicht erstellt werden.");

        var accountId = createResult.Value.Id;

        // 5. Verifizierungslink generieren
        var token = Guid.NewGuid().ToString("N");
        var tokenResult = await _tokenRepo.CreateAsync(accountId, token);
        if (tokenResult.IsFailed)
            return Result.Fail("Token konnte nicht gespeichert werden.");

        var link = $"{_jwtOptions.BackendBaseUrl}api/authenticate/verify?token={token}";
        var body = $"""
                <p>Hallo {user.Firstname},</p>
                <p>du wurdest als Mitglied der Organisation <b>{organization.Name}</b> hinzugefügt.</p>
                <p>Bestätige deine neue Mitgliedschaft über diesen Link:</p>
                <p><a href="{link}">Mitgliedschaft bestätigen</a></p>
                <p><small>Dieser Link ist 48 Stunden gültig.</small></p>
                """;

        await _emailService.SendAsync(newEmail, "Neue Mitgliedschaft bestätigen", body);
        return Result.Ok();
    }
    
    /// <summary>
    /// Generiert ein JWT-Token für ein Benutzerkonto.
    /// </summary>
    /// <param name="account">Das Benutzerkonto, für das das Token erstellt wird.</param>
    /// <returns>Ein gültiges JWT als Zeichenkette.</returns>
    private string GenerateJwtToken(Account account)
    {
        var claims = new[]
        {
            // UserId (wer bin ich? – bleibt Sub und NameIdentifier)
            new Claim(JwtRegisteredClaimNames.Sub, account.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, account.UserId.ToString()),

            // AccountId (in welchem Account bin ich eingeloggt?)
            new Claim("accountId", account.Id.ToString()),

            // Mail & Orga
            new Claim(JwtRegisteredClaimNames.Email, account.EMail),
            new Claim("org", account.Organization.Domain),
            new Claim("orgName", account.Organization.Name),

            // Rolle
            new Claim(ClaimTypes.Role, account.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpirationTime),
            signingCredentials: credentials
        );

        _logger.LogInformation("JWT wird mit SecretKey erstellt: {Key}", _jwtOptions.SecretKey);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}