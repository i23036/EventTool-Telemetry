namespace ET.Shared.DTOs;

public record EventListDto(
    int EventId, 
    String Name, 
    string StatusDisplay,
    bool CanSubscribe,
    String Description, 
    int Participants, 
    int MaxParticipants, 
    bool IsOrganizer, 
    bool IsSubscribed
    );