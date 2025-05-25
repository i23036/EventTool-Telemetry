using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ET_Frontend.Services.Authentication;

/// <summary>
/// AuthenticationStateProvider für JWT-basierte Authentifizierung.
/// Liest das Token aus dem SessionStorage, analysiert Claims und liefert den aktuellen Authentifizierungszustand.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;
    private const string TokenKey = "authToken";

    public JwtAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    /// <summary>
    /// Gibt den aktuellen Authentifizierungszustand des Benutzers zurück.
    /// Wird von Blazor bei Komponenten mit [Authorize] automatisch aufgerufen.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _sessionStorage.GetItemAsStringAsync(TokenKey);

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            // Token konnte nicht gelesen werden → als nicht authentifiziert behandeln
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Speichert das neue JWT und informiert die App über den geänderten Auth-State.
    /// </summary>
    public async Task MarkUserAsAuthenticated(string token)  
    {
        await _sessionStorage.SetItemAsStringAsync(TokenKey, token);
        NotifyAuthenticationStateChanged();
    }

    /// <summary>
    /// Wird aufgerufen, wenn z. B. Login oder Logout erfolgt ist und der Auth-Zustand sich geändert hat.
    /// Benachrichtigt alle Abonnenten (z. B. Layouts, [Authorize]-Views).
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
