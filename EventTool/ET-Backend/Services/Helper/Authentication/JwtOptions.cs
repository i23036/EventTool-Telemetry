namespace ET_Backend.Services.Helper.Authentication;
/// <summary>
/// Stellt die Konfigurationsoptionen für die JWT-Authentifizierung bereit.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Der Aussteller (Issuer) des Tokens, z. B. dein Backend-Servername oder eine URI.
    /// </summary>
    public String Issuer { get; init; }

    /// <summary>
    /// Der Empfänger (Audience), für den das Token bestimmt ist, z. B. dein Frontend.
    /// </summary>
    public String Audiece { get; init; }

    /// <summary>
    /// Die Gültigkeitsdauer des Tokens in Stunden.
    /// </summary>
    public int ExperationTime { get; init; }

    /// <summary>
    /// Der geheime Schlüssel, der zum Signieren der Tokens verwendet wird.
    /// </summary>
    public String SecretKey { get; init; }
}