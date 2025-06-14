using ET.Shared.DTOs;
using ET_Frontend.Models.Event;

namespace ET_Frontend.Mapping;

public static class EventListViewMapper
{
    public static EventViewModel ToViewModel(EventListDto dto) => new()
    {
        Id              = dto.EventId,
        Name            = dto.Name,
        Description     = dto.Description,

        StatusDisplay   = dto.StatusDisplay,
        CanSubscribe    = dto.CanSubscribe,

        Participants    = dto.Participants,
        MaxParticipants = dto.MaxParticipants,
        IsOrganizer     = dto.IsOrganizer,
        IsSubscribed    = dto.IsSubscribed
    };
}