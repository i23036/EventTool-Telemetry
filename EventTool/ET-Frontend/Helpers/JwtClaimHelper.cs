using Microsoft.AspNetCore.Components.Authorization;

namespace ET_Frontend.Helpers;

/// <summary>
/// Stellt Hilfsmethoden zum komfortablen Auslesen von JWT-Claims im Frontend bereit.
/// </summary>
public static class JwtClaimHelper
{
    /// <summary>
    /// Liest einen bestimmten Claim aus dem aktuellen Authentifizierungszustand.
    /// </summary>
    /// <param name="provider">Das Authentifizierungs-Provider-Objekt.</param>
    /// <param name="claimType">Der Typ des Claims (z. B. "org", "orgName").</param>
    /// <returns>Der Wert des Claims oder ein leerer String, falls nicht gefunden.</returns>
    public static async Task<string> GetClaimAsync(AuthenticationStateProvider provider, string claimType)
    {
        var authState = await provider.GetAuthenticationStateAsync();
        var user = authState.User;

        // Rückgabe des Claims, falls vorhanden – sonst leer
        return user?.FindFirst(claimType)?.Value ?? string.Empty;
    }

    /// <summary>
    /// Liefert die Domain der Organisation anhand des "org"-Claims.
    /// Fallback ist "demo.org", falls kein Token oder Claim vorhanden.
    /// </summary>
    public static async Task<string> GetUserDomainAsync(AuthenticationStateProvider provider)
    {
        var domain = await GetClaimAsync(provider, "org");
        return string.IsNullOrWhiteSpace(domain) ? "demo.org" : domain;
    }

    /// <summary>
    /// Liefert den ausgeschriebenen Namen der Organisation anhand des "orgName"-Claims.
    /// </summary>
    public static Task<string> GetUserOrganizationNameAsync(AuthenticationStateProvider provider)
        => GetClaimAsync(provider, "orgName");
}