using ET.Shared.DTOs;

namespace ET_Frontend.Services.Authentication;

/// <summary>
/// Stellt Login- und Logout-Methoden für Benutzer bereit.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Führt den Login durch und speichert das Token.
    /// </summary>
    Task<(bool Success, string? Error)> LoginAsync(LoginDto dto);

    /// <summary>
    /// Entfernt das gespeicherte Token und informiert alle Komponenten.
    /// </summary>
    Task LogoutAsync();
}