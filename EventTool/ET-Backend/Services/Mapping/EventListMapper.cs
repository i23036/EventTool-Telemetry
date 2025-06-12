using ET_Backend.Models;
using ET.Shared.DTOs;

namespace ET_Backend.Services.Mapping;

/// <summary>
/// Stellt Mappingmethoden für Event-Listen zur Verfügung.
/// </summary>
public static class EventListMapper
{
    /// <summary>
    /// Wandelt ein Event-Model in ein EventListDto um.
    /// </summary>
    public static EventListDto ToDto(Models.Event evt, Account viewer)
    {
        int count = evt.Participants
            .Where(p =>
                !evt.Organizers.Any(o => o.Id == p.Id) &&
                !evt.ContactPersons.Any(c => c.Id == p.Id))
            .Count();

        return new EventListDto(
            evt.Id,
            evt.Name,
            evt.Description,
            count,
            evt.MaxParticipants,
            evt.Organizers.Contains(viewer),
            evt.Participants.Contains(viewer)
        );
    }

    /// <summary>
    /// Wandelt ein Event-Model in ein EventListDto um – nur anhand der Account-Id.
    /// </summary>
    public static EventListDto ToDto(Models.Event evt, int currentAccountId)
    {
        int count = evt.Participants
            .Where(p =>
                !evt.Organizers.Any(o => o.Id == p.Id) &&
                !evt.ContactPersons.Any(c => c.Id == p.Id))
            .Count();

        bool isOrganizer  = evt.Organizers.Any(o => o.Id == currentAccountId);
        bool isSubscribed = evt.Participants.Any(p => p.Id == currentAccountId);

        return new EventListDto(
            evt.Id,
            evt.Name,
            evt.Description,
            count,
            evt.MaxParticipants,
            isOrganizer,
            isSubscribed
        );
    }

    /// <summary>
    /// Wandelt ein Event-Model in ein EventListDto ohne Benutzerbezug um.
    /// </summary>
    public static EventListDto ToDto(Models.Event evt)
    {
        return new EventListDto(
            evt.Id,
            evt.Name,
            evt.Description,
            evt.Participants.Count,
            evt.MaxParticipants,
            false,
            false
        );
    }
}