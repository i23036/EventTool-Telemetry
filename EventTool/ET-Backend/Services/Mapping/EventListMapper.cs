using ET_Backend.Models;
using ET.Shared.DTOs;

namespace ET_Backend.Services.Mapping;

/// <summary>
/// Stellt Mappingmethoden für Event-Listen zur Verfügung.
/// </summary>
public static class EventListMapper
{
    /// <summary>
    /// Wandelt ein Event-Model in ein EventListDto um
    /// (Variante mit Account-Objekt des Viewers).
    /// </summary>
    public static EventListDto ToDto(Models.Event evt, Account viewer)
    {
        // ► einfache Teilnehmer-Zählung (alle Datensätze in Participants)
        int  count        = evt.Participants.Count;
        bool isOrganizer  = evt.Organizers.Any(o  => o.Id == viewer.Id);
        bool isSubscribed = evt.Participants.Any(p => p.Id == viewer.Id);

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
    /// Wandelt ein Event-Model in ein EventListDto um – nur anhand der Account-Id.
    /// </summary>
    public static EventListDto ToDto(Models.Event evt, int viewerId)
    {
        int count        = evt.Participants.Count;
        bool isOrganizer = evt.Organizers.Any(o  => o.Id == viewerId);
        bool isSubscribed= evt.Participants.Any(p => p.Id == viewerId);

        return new EventListDto(evt.Id, evt.Name, evt.Description,
            count, evt.MaxParticipants,
            isOrganizer, isSubscribed);
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