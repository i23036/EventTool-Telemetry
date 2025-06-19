using ET_Backend.Models;
using ET.Shared.DTOs;

namespace ET_Backend.Services.Mapping;

/// <summary>
/// Stellt Mapping-Methoden zwischen Process-Modell und Process-DTO bereit.
/// </summary>
public static class ProcessMapper
{
    /// <summary>
    /// Konvertiert ein ProcessDto in ein internes Process-Modell.
    /// </summary>
    /// <param name="dto">Das DTO mit den übertragenen Prozessdaten.</param>
    public static Process ToModel(ProcessDto dto)
    {
        return new Process
        {
            Id = dto.Id,
            EventId   = dto.Id,
            ProcessSteps = dto.ProcessSteps
                .Select(ProcessStepMapper.ToModel)
                .ToList()
        };
    }

    /// <summary>
    /// Konvertiert ein Process-Modell in ein ProcessDto.
    /// </summary>
    /// <param name="process">Das interne Prozessobjekt.</param>
    public static ProcessDto ToDto(Process process)
    {
        return new ProcessDto(
            process.Id,
            process.ProcessSteps
                .Select(ProcessStepMapper.ToDto)
                .ToList()
        );
    }
}
