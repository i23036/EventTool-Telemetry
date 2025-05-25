namespace ET.Shared.DTOs;

public record MembershipDto(
    int    AccountId,
    int    OrganisationId,
    string OrganisationName,
    string Email);