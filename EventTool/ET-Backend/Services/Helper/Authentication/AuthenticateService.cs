using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ET_Backend.Models;
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
    private IAccountRepository _accountRepository;
    private IUserRepository _userRepository;
    private IOrganizationRepository _organizationRepository;
    private JwtOptions _jwtOptions;

    /// <summary>
    /// Erstellt eine neue Instanz des <see cref="AuthenticateService"/>.
    /// </summary>
    /// <param name="accountRepository">Das Repository für Benutzerkonten.</param>
    /// <param name="jwtOptions">Konfiguration für das JWT-Token.</param>
    public AuthenticateService(IAccountRepository accountRepository, IUserRepository userRepository, 
        IOrganizationRepository organizationRepository, IOptions<JwtOptions> jwtOptions)
    {
        _accountRepository = accountRepository;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Versucht, einen Benutzer zu authentifizieren und gibt ein JWT zurück, wenn erfolgreich.
    /// </summary>
    /// <param name="eMail">Die E-Mail-Adresse des Benutzers.</param>
    /// <param name="password">Das Passwort des Benutzers.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem JWT als Zeichenkette bei Erfolg oder einer Fehlermeldung.
    /// </returns>
    public async Task<Result<string>> LoginUser(string eMail, string password)
    {
        Result<bool> accountExists = await _accountRepository.AccountExists(eMail);
        if (accountExists.IsSuccess && accountExists.Value)
        {
            Result<Account> account = await _accountRepository.GetAccount(eMail);
            string passwordHash = account.Value.User.Password;
            if (account.IsSuccess && passwordHash == password)
            {
                string token = GenerateJwtToken(account.Value);
                return Result.Ok(token);
            }
            else
            {
                return Result.Fail<string>("Password does not match");
            }
        }
        else
        {
            return Result.Fail<string>("User does not exist");
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
    public async Task<Result<String>> RegisterUser(String firstname, String lastname, String eMail, String password)
    {
        Result<bool> accountExists = await _accountRepository.AccountExists(eMail);
        if (accountExists.IsSuccess && !accountExists.Value)
        {
            String eMailDomain = eMail.Substring(eMail.LastIndexOf('@') + 1);

            Result<bool> organizationExists = await _organizationRepository.OrganizationExists(eMailDomain);
            if (organizationExists.IsSuccess && organizationExists.Value)
            {

                Result<User> user = await _userRepository.CreateUser(firstname, lastname, password);
                Result<Models.Organization> organization = await _organizationRepository.GetOrganization(eMailDomain);
                await _accountRepository.CreateAccount(eMail, organization.Value, Role.Member, user.Value);

                if (user.IsSuccess && organization.IsSuccess)
                {
                    return Result.Ok("User added successfully");
                }
                else
                {
                    return Result.Fail("Database failure");
                }
            }
            else
            {
                return Result.Fail("No organization exists for this E-Mail");
            }
        }
        else
        {
            return Result.Fail("User already exists");
        }
    }


    /// <summary>
    /// Generiert ein JWT-Token für ein Benutzerkonto.
    /// </summary>
    /// <param name="account">Das Benutzerkonto, für das das Token erstellt wird.</param>
    /// <returns>Ein gültiges JWT als Zeichenkette.</returns>
    private string GenerateJwtToken(Account account)
    {
        Claim[] claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Email, account.EMail),
            
            // TODO: Mehr hinzufügen
        };

        SigningCredentials signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256
            );

        JwtSecurityToken token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audiece,
            claims,
            null,
            DateTime.UtcNow.AddHours(_jwtOptions.ExperationTime),
            signingCredentials
            );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}