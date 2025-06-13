using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ET_Backend.Services.Helper;

/// <summary>
/// Robustere Helfer zum Auslesen essentieller JWT-Claims.
/// </summary>
public static class TokenHelper
{
    public static string GetEmail(ClaimsPrincipal user) =>
        // 1) Standard            2) Alternative             3) Plain "email"
        user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? user.FindFirst(ClaimTypes.Email)?.Value
        ?? user.FindFirst("email")?.Value
        ?? string.Empty;

    public static string GetRole(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Role)?.Value
        ?? user.FindFirst("role")?.Value
        ?? string.Empty;

    public static string GetOrgDomain(ClaimsPrincipal user) =>
        user.FindFirst("org")?.Value
        ?? user.FindFirst("orgDomain")?.Value
        ?? string.Empty;
}