using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ET_Backend.Models;
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
    private IAccountRepository _repository;
    private JwtOptions _jwtOptions;

    /// <summary>
    /// Erstellt eine neue Instanz des <see cref="AuthenticateService"/>.
    /// </summary>
    /// <param name="repository">Das Repository für Benutzerkonten.</param>
    /// <param name="jwtOptions">Konfiguration für das JWT-Token.</param>
    public AuthenticateService(IAccountRepository repository, IOptions<JwtOptions> jwtOptions)
    {
        _repository = repository;
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
        if (await _repository.AccountExists(eMail))
        {
            string passwordHash = await _repository.GetPasswordHash(eMail);
            if (passwordHash == password)
            {
                string token = GenerateJwtToken(await _repository.GetAccount(eMail));
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