using ET.Shared.DTOs;
using ET_Backend.Models;
using ET_Backend.Services.Event;
using ET_Backend.Services.Helper.Authentication;
using ET_Backend.Services.Mapping;
using ET_Backend.Services.Organization;
using ET_Backend.Services.Person;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
        private readonly IOrganizationService _organizationService;

        public EventController(IEventService eventService, IUserService userService, IOrganizationService organizationService)
        {
            _eventService = eventService;
            _userService = userService;
            _organizationService = organizationService;
        }

        [HttpGet("eventList/{domain}")]
        [Authorize]
        public async Task<IActionResult> EventList(string domain)
        {
            // Organisation holen
            var orgResult = await _organizationService.GetOrganization(domain);
            if (orgResult.IsFailed)
                return Unauthorized("Organisation nicht gefunden.");

            var org = orgResult.Value;

            // Benutzer holen (z. B. für IsOrganizer, IsSubscribed)
            var account = await _userService.GetCurrentUserAsync(User);

            // Events holen
            var result = await _eventService.GetEventsFromOrganization(org.Id);

            if (result.IsSuccess)
            {
                var dtoList = result.Value
                    .Select(e => EventListMapper.ToDto(e, account))
                    .ToList();

                return Ok(dtoList);
            }

            return BadRequest(result.Errors);
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
