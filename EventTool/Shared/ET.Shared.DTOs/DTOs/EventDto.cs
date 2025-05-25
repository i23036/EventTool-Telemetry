namespace ET.Shared.DTOs;

public record EventDto(
    String Name, 
    String Description, 
    String Location, 
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
    bool IsBlueprint
    );