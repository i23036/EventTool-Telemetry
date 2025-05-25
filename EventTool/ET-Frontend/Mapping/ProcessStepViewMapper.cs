using ET.Shared.DTOs;
using ET_Frontend.Models;
using ET_Frontend.Models.Enums;

namespace ET_Frontend.Mapping;

/// <summary>
/// Enthält Mapping-Methoden zwischen ProcessStepDto und ProcessStepViewModel.
/// </summary>
public static class ProcessStepViewMapper
{
    /// <summary>
    /// Wandelt einen ProcessStepDto in ein ViewModel zur Anzeige/Bearbeitung um.
    /// </summary>
    public static ProcessStepViewModel ToViewModel(ProcessStepDto dto)
    {
        

        return new ProcessStepViewModel
        {
            Id = dto.Id,
            TypeName = dto.TypeName,
            Type = (ProcessStepType)dto.TypeE,
            Trigger = (ProcessStepTrigger)dto.TriggerE,
            Condition = (ProcessStepCondition)dto.ConditionE,
            OffsetInHours = dto.OffsetInHours
        };
    }

    /// <summary>
    /// Wandelt ein ViewModel zurück in einen ProcessStepDto zur API-Übertragung.
    /// </summary>
    public static ProcessStepDto ToDto(ProcessStepViewModel vm)
    {
        return new ProcessStepDto(
            vm.Id,
            vm.TypeName,
            (int)vm.Type,
            (int)vm.Trigger,
            (int)vm.Condition,
            vm.OffsetInHours
        );
    }
}