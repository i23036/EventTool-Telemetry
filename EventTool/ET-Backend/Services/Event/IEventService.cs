using FluentResults;

namespace ET_Backend.Services.Event;

public interface IEventService
{
    public Task<Result<List<Models.Event>>> GetEventsFromOrganization(int organizationId);
    public Task<Result<List<Models.Event>>> GetEventsFromOrganization(String domain);

    public Task<Result> SubscribeToEvent(int accountId, int eventId);
    public Task<Result> UnsubscribeToEvent(int accountId, int eventId);

    public Task<Result<Models.Event>> CreateEvent(Models.Event newEvent, int organizationId);
    public Task<Result> DeleteEvent(int eventId);
}