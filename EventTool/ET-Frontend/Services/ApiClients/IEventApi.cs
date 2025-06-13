using ET.Shared.DTOs;

namespace ET_Frontend.Services.ApiClients;

public interface IEventApi
{
    /// <summary> Meldet den aktuell eingeloggten Benutzer zu einem Event an. </summary>
    Task<bool> SubscribeAsync(int eventId);

    /// <summary> Meldet den aktuell eingeloggten Benutzer von einem Event ab. </summary>
    Task<bool> UnsubscribeAsync(int eventId);

    /// <summary> Erstellt ein neues Event. </summary>
    Task<bool> CreateEventAsync(EventDto dto);

    Task<bool> UpdateEventAsync(EventDto dto);

    /// <summary> Lädt eine Event-Detail-Seite. </summary>
    Task<EventDto?> GetEventAsync(int eventId);
    Task<bool> RemoveParticipantAsync(int eventId, int accountId);
}