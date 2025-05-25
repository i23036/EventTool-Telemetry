using ET.Shared.DTOs;
using ET_Frontend.Models.Event;

namespace ET_Frontend.Mapping;

/// <summary>
/// Mapped Event-DTOs zu ViewModels.
/// </summary>
public static class EventListViewMapper
{
    public static EventViewModel ToViewModel(EventListDto dto)
    {
        return new EventViewModel
        {
            EventId = dto.EventId,
            Name = dto.Name,
            Description = dto.Description,
            Participants = dto.Participants,
            MaxParticipants = dto.MaxParticipants,
            IsOrganizer = dto.IsOrganizer,
            IsSubscribed = dto.IsSubscribed
        };
    }
}