namespace ET.Shared.DTOs;

public record ProcessDto(
    int Id, 
    int EventId,
    List<ProcessStepDto> ProcessSteps
    );