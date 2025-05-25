using ET_Frontend.Models.AccountManagement;
using ET.Shared.DTOs;

namespace ET_Frontend.Mapping;

public static class DtoMembershipMapper
{
    public static MembershipViewModel FromDto(MembershipDto dto) => new()
    {
        AccountId        = dto.AccountId,
        OrganisationName = dto.OrganisationName,
        Email            = dto.Email,
        OrganisationId   = dto.OrganisationId
    };
}