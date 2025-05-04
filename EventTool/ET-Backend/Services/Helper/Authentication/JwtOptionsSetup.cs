using Microsoft.Extensions.Options;

namespace ET_Backend.Services.Helper.Authentication;
/// <summary>
/// Bindet die JWT-Konfiguration aus der Konfigurationsdatei (z. B. appsettings.json) an ein <see cref="JwtOptions"/>-Objekt.
/// </summary>
public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private IConfiguration _configuration;

    /// <summary>
    /// Erstellt eine neue Instanz von <see cref="JwtOptionsSetup"/> mit Zugriff auf die Konfiguration.
    /// </summary>
    /// <param name="configuration">Die Anwendungs-Konfiguration.</param>
    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Bindet die Konfiguration aus dem Abschnitt "Jwt" an die übergebenen <see cref="JwtOptions"/>.
    /// </summary>
    /// <param name="options">Das <see cref="JwtOptions"/>-Objekt, das befüllt werden soll.</param>
    public void Configure(JwtOptions options)
    {
        _configuration.GetSection("Jwt").Bind(options);
    }
}