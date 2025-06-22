using ET.Shared.DTOs.Enums;

namespace ET.Shared.DTOs;

public record ProcessStepDto(
    int Id,
    string Name,
    ProcessStepTrigger Trigger,
    ProcessStepAction Action,
    int? Offset,
    int? TriggeredByStepId,
    // nur für Action = SendEmail
    string? Subject,
    string? Body
);