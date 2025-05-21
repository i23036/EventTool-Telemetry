using ET_Backend.Models;
using ET.Shared.DTOs;
using ET_Backend.Services.Event;
using ET_Backend.Services.Helper.Authentication;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ET_Backend.Services.Person;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    /// <summary>
    /// Bietet API-Endpunkte zur Verwaltung von Event-Daten.
    /// </summary>
    [Route("api/event")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IUserService _userService;

        public EventController(IEventService eventService, IUserService userService)
        {
            _eventService = eventService;
            _userService = userService;
        }

        [HttpGet("eventList")]
        public async Task<IActionResult> EventList()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || user.Organization == null)
                return Unauthorized("Ungültiger Benutzer oder keine Organisation gefunden.");

            Result<List<Event>> result = await _eventService.GetEventsFromOrganization(user.Organization.Id);

            if (result.IsSuccess)
            {
                List<EventListDto> dtoList = new List<EventListDto>();
                foreach (Event currentEvent in result.Value)
                {
                    EventListDto newDto = new EventListDto(
                        currentEvent.Id,
                        currentEvent.Name,
                        currentEvent.Description,
                        currentEvent.Participants.Count,
                        currentEvent.MaxParticipants,
                        currentEvent.Organizers.Contains(user),
                        currentEvent.Participants.Contains(user)
                    );
                    dtoList.Add(newDto);
                }
                return Ok(dtoList);
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPut("subsrcibeTo{eventId}")]
        public async Task<IActionResult> SubscribeToEvent(int eventId)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || user.Organization == null)
                return Unauthorized("Ungültiger Benutzer oder keine Organisation gefunden.");

            Result result = await _eventService.SubscribeToEvent(user.Id, eventId);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPut("unsubsrcibeTo{eventId}")]
        public async Task<IActionResult> UnsubscribeToEvent(int eventId)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || user.Organization == null)
                return Unauthorized("Ungültiger Benutzer oder keine Organisation gefunden.");

            Result result = await _eventService.UnsubscribeToEvent(user.Id, eventId);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost("createEvent")]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto value)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || user.Organization == null)
                return Unauthorized("Ungültiger Benutzer oder keine Organisation gefunden.");

            Event newEvent = new Event();
            newEvent.Name = value.Name;
            newEvent.Description = value.Description;
            newEvent.Organization = user.Organization;
            newEvent.Organizers.Add(user);
            // TODO: Alle Werte einfügen

            Result<Event> result = await _eventService.CreateEvent(newEvent);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || user.Organization == null)
                return Unauthorized("Ungültiger Benutzer oder keine Organisation gefunden.");

            Result result = await _eventService.DeleteEvent(eventId);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
