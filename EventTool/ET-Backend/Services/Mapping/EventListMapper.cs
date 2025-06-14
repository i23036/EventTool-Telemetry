
using ET.Shared.DTOs;
using ET.Shared.DTOs.Enums;

namespace ET_Backend.Services.Mapping;

public static class EventListMapper
{
    public static EventListDto ToDto(Models.Event evt, string viewerEmail)
    {
        int  count        = evt.Participants.Count;
        bool isOrganizer  = evt.Organizers.Any(o  => o.EMail == viewerEmail);
        bool isSubscribed = evt.Participants.Any(p => p.EMail == viewerEmail);

        return new EventListDto(
            evt.Id,
            evt.Name,
            evt.Status.ToString(),                // StatusDisplay
            evt.Status == EventStatus.Offen,      // CanSubscribe
            evt.Description,
            count,
            evt.MaxParticipants,
            isOrganizer,
            isSubscribed
        );
    }
}
