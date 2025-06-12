using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ET_Backend.Services.Helper;

public static class TokenHelper
{
    public static int? GetAccountId(ClaimsPrincipal user)
    {
        var accId = user.FindFirst("accountId")?.Value;
        return int.TryParse(accId, out var id) ? id : null;
    }

    public static int? GetUserId(ClaimsPrincipal user)
    {
        var uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(uid, out var id) ? id : null;
    }

    public static string? GetEmail(ClaimsPrincipal user)
        => user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public static string? GetOrgDomain(ClaimsPrincipal user)
        => user.FindFirst("org")?.Value;

    public static string? GetOrgName(ClaimsPrincipal user)
        => user.FindFirst("orgName")?.Value;

    public static string? GetRole(ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value;

    public static bool IsOwner(ClaimsPrincipal user)
        => GetRole(user)?.Equals("Owner", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsOrganizer(ClaimsPrincipal user)
        => GetRole(user)?.Equals("Organisator", StringComparison.OrdinalIgnoreCase) == true;
}