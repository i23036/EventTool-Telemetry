using ET_Backend.Models;
using ET.Shared.DTOs;
using ET.Shared.DTOs.Enums;

namespace ET_Backend.Services.Mapping;

/// <summary>
/// Stellt Mapping-Methoden zwischen ProcessStep-Modell und ProcessStep-DTO bereit.
/// </summary>
public static class ProcessStepMapper
{
    /// <summary>
    /// Konvertiert ein ProcessStepDto in ein internes ProcessStep-Modell.
    /// </summary>
    /// <param name="dto">Das DTO mit den übertragenen Prozessschritt-Daten.</param>
    public static ProcessStep ToModel(ProcessStepDto dto)
    {
        return new ProcessStep
        {
            Id = dto.Id,
            Name = dto.Name,
            Trigger = (ProcessStepTrigger)dto.Trigger,
            Action = (ProcessStepAction)dto.Action,
            Offset = dto.Offset,
            TriggeredByStepId = dto.TriggeredByStepId
        };
    }

    /// <summary>
    /// Konvertiert ein ProcessStep-Modell in ein ProcessStepDto.
    /// </summary>
    /// <param name="model">Das interne Prozessschritt-Objekt.</param>
    public static ProcessStepDto ToDto(ProcessStep model)
    {
        return new ProcessStepDto(
            model.Id,
            model.Name,
            model.Trigger,
            model.Action,
            model.Offset,
            model.TriggeredByStepId
        );
    }
}
