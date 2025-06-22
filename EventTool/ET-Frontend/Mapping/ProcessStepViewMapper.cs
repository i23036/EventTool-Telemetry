using ET.Shared.DTOs;
using ET_Frontend.Models;
using ET.Shared.DTOs.Enums;

namespace ET_Frontend.Mapping;

/// <summary>
/// Enthält Mapping-Methoden zwischen ProcessStepDto und ProcessStepViewModel.
/// </summary>
public static class ProcessStepViewMapper
{
    /// <summary>
    /// Wandelt einen ProcessStepDto in ein ViewModel zur Anzeige/Bearbeitung um.
    /// </summary>
    public static ProcessStepViewModel ToViewModel(ProcessStepDto dto) => new()
    {
        Id                = dto.Id,
        Name              = dto.Name,
        Trigger           = dto.Trigger,
        Action            = dto.Action,
        Offset            = dto.Offset ?? 0,
        TriggeredByStepId = dto.TriggeredByStepId,
        Subject           = dto.Subject,
        Body              = dto.Body
    };

    /// <summary>
    /// Wandelt ein ViewModel zurück in einen ProcessStepDto zur API-Übertragung.
    /// </summary>
    public static ProcessStepDto ToDto(ProcessStepViewModel vm) => new(
        vm.Id,
        vm.Name,
        vm.Trigger,
        vm.Action,
        vm.Offset == 0 ? null : vm.Offset,
        vm.TriggeredByStepId,
        vm.Subject,
        vm.Body
    );
}