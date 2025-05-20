using System.Net.Http.Json;
using Blazored.SessionStorage;
using ET.Shared.DTOs;
using ET_Frontend.Services.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Services.Authentication;

/// <summary>
/// Implementierung des Login-Dienstes für JWT-basierte Authentifizierung.
/// </summary>
public class LoginService : ILoginService
{
    private readonly HttpClient _http;
    private readonly ISessionStorageService _storage;
    private readonly AuthenticationStateProvider _authProvider;
    private readonly NavigationManager _nav;

    private const string TokenKey = "authToken";

    public LoginService(HttpClient http, ISessionStorageService storage,
        AuthenticationStateProvider authProvider, NavigationManager nav)
    {
        _http = http;
        _storage = storage;
        _authProvider = authProvider;
        _nav = nav;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(LoginDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/authenticate/login", dto);

            if (!response.IsSuccessStatusCode)
                return (false, await response.Content.ReadAsStringAsync());

            var token = await response.Content.ReadAsStringAsync();
            await _storage.SetItemAsStringAsync(TokenKey, token);

            if (_authProvider is JwtAuthenticationStateProvider jwt)
                jwt.NotifyAuthenticationStateChanged();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"[EX] {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync(TokenKey);

        if (_authProvider is JwtAuthenticationStateProvider jwt)
            jwt.NotifyAuthenticationStateChanged();

        _nav.NavigateTo("/login", forceLoad: true);
    }
}