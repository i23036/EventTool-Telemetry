namespace ET.Shared.DTOs;

public record EventDto(String Name, String Description, DateOnly StartDate, DateOnly EndDate, int MinParticipants, int MaxParticipants, bool IsBlueprint);