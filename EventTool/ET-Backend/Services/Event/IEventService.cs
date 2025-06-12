using ET.Shared.DTOs;
using FluentResults;
using System.Security.Claims;

namespace ET_Backend.Services.Event;

public interface IEventService
{
    Task<Result<Models.Event>> CreateEvent(Models.Event newEvent, int organizationId);
    Task<Result> UpdateEventAsync(EventDto dto, ClaimsPrincipal user);
    public Task<Result<List<Models.Event>>> GetEventsFromOrganization(int organizationId);
    public Task<Result<List<Models.Event>>> GetEventsFromOrganization(String domain);

    public Task<Result> SubscribeToEvent(int accountId, int eventId);
    public Task<Result> UnsubscribeToEvent(int accountId, int eventId);

    public Task<Result> DeleteEvent(int eventId);
    public Task<Result<Models.Event>> GetEvent(int eventId);
}