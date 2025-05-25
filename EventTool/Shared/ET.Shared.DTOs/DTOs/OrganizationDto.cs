namespace ET.Shared.DTOs;

public record OrganizationDto(
    int Id,
    string Name,
    string Domain,
    string Description,
    string OrgaPicAsBase64,
    string OwnerFirstName,
    string OwnerLastName,
    string OwnerEmail,
    string InitialPassword
);