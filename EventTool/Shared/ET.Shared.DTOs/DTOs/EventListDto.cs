namespace ET.Shared.DTOs;

public record EventListDto(int EventId, String Name, String Description, int Participants, int MaxParticipants, bool IsOrganizer, bool IsSubscribed);