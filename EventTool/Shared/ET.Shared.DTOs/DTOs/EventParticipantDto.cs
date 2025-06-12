namespace ET.Shared.DTOs;

public record EventParticipantDto(
    int AccountId,
    string Firstname,
    string Lastname,
    string Email
);