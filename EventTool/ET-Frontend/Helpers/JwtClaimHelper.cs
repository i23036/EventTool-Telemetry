using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

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

    /// <summary>
    /// Gibt die Benutzer-ID anhand des "sub"-Claims zurück.
    /// </summary>
    /// <param name="provider">Das Authentifizierungs-Provider-Objekt.</param>
    /// <returns>Die Benutzer-ID als int, oder -1 bei Fehler.</returns>
    public static async Task<int> GetUserIdAsync(AuthenticationStateProvider provider)
    {
        var claim = await GetClaimAsync(provider, ClaimTypes.NameIdentifier); // "sub" oder standardisiert
        return int.TryParse(claim, out var id) ? id : -1;
    }
}
