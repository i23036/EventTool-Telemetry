using ET_Backend.Repository.Event;
using FluentResults;

namespace ET_Backend.Services.Event;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    // TODO: Methoden implementieren

    public async Task<Result<List<Models.Event>>> GetEventsFromOrganization(int organizationId)
    {
        return await _eventRepository.GetEventsByOrganizationId(organizationId);
    }

    public async Task<Result<List<Models.Event>>> GetEventsFromOrganization(String domain)
    {
        return null;
    }

    public async Task<Result> SubscribeToEvent(int accountId, int eventId)
    {
        return null;
    }

    public async Task<Result> UnsubscribeToEvent(int accountId, int eventId)
    {
        return null;
    }

    public async Task<Result<Models.Event>> CreateEvent(Models.Event newEvent)
    {
        return null;
    }

    public async Task<Result> DeleteEvent(int eventId)
    {
        return null;
    }
}