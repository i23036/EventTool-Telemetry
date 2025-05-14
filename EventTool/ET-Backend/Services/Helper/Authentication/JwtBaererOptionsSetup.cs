using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ET_Backend.Services.Helper.Authentication;
/// <summary>
/// Konfiguriert die Optionen für die JWT-Authentifizierung.
/// </summary>
public class JwtBaererOptionsSetup : IConfigureOptions<JwtBearerOptions>
{
    private JwtOptions _jwtOptions;

    /// <summary>
    /// Erstellt eine neue Instanz von <see cref="JwtBaererOptionsSetup"/> mit den angegebenen JWT-Optionen.
    /// </summary>
    /// <param name="options">Die konfigurierten JWT-Optionen.</param>
    public JwtBaererOptionsSetup(IOptions<JwtOptions> options)
    {
        _jwtOptions = options.Value;
    }

    /// <summary>
    /// Konfiguriert die Tokenvalidierungsparameter für die JWT-Authentifizierung.
    /// </summary>
    /// <param name="options">Die zu konfigurierenden <see cref="JwtBearerOptions"/>.</param>
    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey))
        };
    }
}