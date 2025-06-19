using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ET_Frontend.Helpers;

/// <summary>
/// Stellt Hilfsmethoden zum komfortablen und konsistenten Auslesen von JWT-Claims im Frontend bereit.
/// </summary>
public static class JwtClaimHelper
{
    /// <summary>
    /// Liest einen bestimmten Claim aus dem aktuellen Authentifizierungszustand.
    /// </summary>
    public static async Task<string> GetClaimAsync(AuthenticationStateProvider provider, string claimType)
    {
        var authState = await provider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user?.FindFirst(claimType)?.Value ?? string.Empty;
    }

    /// <summary>
    /// Liefert die Benutzer-ID (UserId) – wer bin ich?
    /// </summary>
    public static async Task<int> GetUserIdAsync(AuthenticationStateProvider provider)
    {
        var id = await GetClaimAsync(provider, ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId) ? userId : -1;
    }

    /// <summary>
    /// Liefert die aktuell genutzte Account-ID – in welchem Account bin ich eingeloggt?
    /// </summary>
    public static async Task<int> GetAccountIdAsync(AuthenticationStateProvider provider)
    {
        var id = await GetClaimAsync(provider, "accountId");
        return int.TryParse(id, out var accountId) ? accountId : -1;
    }

    /// <summary>
    /// Liefert die Domain der aktuellen Organisation (org).
    /// </summary>
    public static async Task<string> GetUserDomainAsync(AuthenticationStateProvider provider)
    {
        var domain = await GetClaimAsync(provider, "org");
        return string.IsNullOrWhiteSpace(domain) ? "demo.org" : domain; // Fallback für Login-Page
    }

    /// <summary>
    /// Liefert den Namen der aktuellen Organisation (orgName).
    /// </summary>
    public static Task<string> GetUserOrganizationNameAsync(AuthenticationStateProvider provider)
        => GetClaimAsync(provider, "orgName");

    /// <summary>
    /// Liefert die Rolle des aktuell genutzten Accounts.
    /// </summary>
    public static async Task<string> GetUserRoleAsync(AuthenticationStateProvider provider)
    {
        var role = await GetClaimAsync(provider, ClaimTypes.Role);
        return string.IsNullOrWhiteSpace(role) ? string.Empty : role.Trim().ToLower();
    }
}