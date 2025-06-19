
using ET.Shared.DTOs;
using ET.Shared.DTOs.Enums;

namespace ET_Backend.Services.Mapping;

public static class EventListMapper
{
    public static EventListDto ToDto(Models.Event evt, string viewerEmail)
    {
        bool isOrganizer  = evt.Organizers.Any(o => o.EMail == viewerEmail);
        bool isSubscribed = evt.Participants.Any(p => p.EMail == viewerEmail);
        bool isDraft      = evt.Status == EventStatus.Entwurf;
        bool isPublic     = evt.Status is EventStatus.Offen
            or EventStatus.Geschlossen
            or EventStatus.Abgesagt
            or EventStatus.Archiviert;

        return new EventListDto(
            evt.Id,
            evt.Name,
            evt.Status.ToString(),
            evt.Status == EventStatus.Offen,
            evt.Description,
            evt.Participants.Count,
            evt.MaxParticipants,
            isOrganizer,
            isSubscribed,
            isDraft,
            isOrganizer,
            isPublic
        );
    }
}
