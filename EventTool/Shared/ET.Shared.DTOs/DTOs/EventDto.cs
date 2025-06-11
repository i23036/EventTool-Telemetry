using ET.Shared.DTOs.Enums;

namespace ET.Shared.DTOs;

public record EventDto(
    int Id,
    string Name,
    string EventType,
    string Description,
    string Location,
    List<string> Organizers,
    List<string> ContactPersons,
    int ProcessId,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int MinParticipants,
    int MaxParticipants,
    DateOnly RegistrationStart,
    DateOnly RegistrationEnd,
    EventStatus Status,
    bool IsBlueprint
);