using FluentResults;

namespace ET_Backend.Repository.Event;

public interface IEventRepository
{
    public Task<Result<bool>> EventExists(int eventId);
    public Task<Result<Models.Event>> CreateEvent(Models.Event newEvent, int organizationId);
    public Task<Result> DeleteEvent(int eventId);
    public Task<Result<Models.Event>> GetEvent(int eventId);
    public Task<Result<List<Models.Event>>> GetEventsByOrganization(int organizationId);
    public Task<Result> EditEvent(Models.Event currentEvent);
}