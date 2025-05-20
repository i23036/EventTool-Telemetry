using Microsoft.AspNetCore.Components.Authorization;

namespace ET_Frontend.Helpers;

/// <summary>
/// Stellt Hilfsmethoden zum Auslesen von Claims aus dem aktuellen Authentifizierungszustand bereit.
/// </summary>
public static class JwtClaimHelper
{
    /// <summary>
    /// Liefert die Domain der Organisation anhand des "org"-Claims.
    /// Fallback ist "demo.org", falls kein Token oder Claim vorhanden.
    /// </summary>
    public static async Task<string> GetUserDomainAsync(AuthenticationStateProvider provider)
    {
        var authState = await provider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user?.FindFirst("org")?.Value ?? "demo.org";
    }
}