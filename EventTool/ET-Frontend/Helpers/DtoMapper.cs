using ET.Shared.DTOs;
using ET_Frontend.Models;

namespace ET_Frontend.Helpers;

/// <summary>
/// Stellt Methoden zur Konvertierung zwischen OrganizationDto und OrgaChangeModel bereit.
/// </summary>
public static class DtoMapper
{
    /// <summary>
    /// Wandelt ein OrganizationDto in ein ViewModel um.
    /// Fügt das Base64-Präfix nur hinzu, wenn ein Logo vorhanden ist.
    /// </summary>
    /// <param name="dto">Das empfangene DTO vom Backend.</param>
    /// <returns>Ein vollständiges ViewModel zur Verwendung in der Razor-Komponente.</returns>
    public static OrgaChangeModel FromDto(OrganizationDto dto) => new()
    {
        orgaName = dto.Name,
        description = dto.Description,
        domain = dto.Domain,

        // Wenn kein Bild vorhanden ist (NULL in DB), wird nichts angezeigt.
        orgaPicBase64 = string.IsNullOrWhiteSpace(dto.OrgaPicAsBase64)
            ? null
            : $"data:image/png;base64,{dto.OrgaPicAsBase64}"
    };

    /// <summary>
    /// Wandelt das ViewModel zurück in ein DTO für die API.
    /// Entfernt den Base64-Prefix.
    /// </summary>
    public static OrganizationDto ToDto(OrgaChangeModel model) =>
        new(model.orgaName, model.domain, model.description, StripPrefix(model.orgaPicBase64));

    /// <summary>
    /// Entfernt den Prefix ("data:image/...") aus einem Base64-String.
    /// </summary>
    private static string StripPrefix(string base64) =>
        base64?.Contains(',') == true ? base64.Split(',')[1] : base64;
}