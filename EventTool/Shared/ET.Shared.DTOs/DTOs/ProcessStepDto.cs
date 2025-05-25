namespace ET.Shared.DTOs;

public record ProcessStepDto(int Id, string TypeName, int TypeE, int TriggerE, int ConditionE, int OffsetInHours);
