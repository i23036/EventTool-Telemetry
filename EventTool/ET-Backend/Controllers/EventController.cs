using System.Security.Claims;
using ET.Shared.DTOs;
using ET_Backend.Models;
using ET_Backend.Services.Event;
using ET_Backend.Services.Helper;
using ET_Backend.Services.Mapping;
using ET_Backend.Services.Organization;
using ET_Backend.Services.Person;
using ET.Shared.DTOs.Enums;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IAccountService _accountService;
        private readonly IOrganizationService _organizationService;
        
        public EventController(IEventService eventService, IUserService userService,IAccountService accountService, IOrganizationService organizationService)
        {
            _eventService = eventService;
            _userService = userService;
            _accountService = accountService;
            _organizationService = organizationService;
        }

        [HttpGet("eventList/{domain}")]
        [Authorize]
        public async Task<IActionResult> EventList(string domain)
        {
            var user  = User;
            var email = TokenHelper.GetEmail(user);
            var role  = TokenHelper.GetRole(user);               // Owner | Organizer | Member

            // Events holen (alle)
            var orgRes = await _organizationService.GetOrganization(domain);
            if (orgRes.IsFailed) return Unauthorized();

            var evRes = await _eventService.GetEventsFromOrganization(orgRes.Value.Id);
            if (evRes.IsFailed) return BadRequest(evRes.Errors);

            IEnumerable<Event> filtered;

            if (role == "Owner")
            {
                filtered = evRes.Value;
            }
            else if (role == "Organizer")
            {
                filtered = evRes.Value.Where(e =>
                    e.Status != EventStatus.Entwurf ||
                    e.Organizers.Any(o => string.Equals(o.EMail, email, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                filtered = evRes.Value.Where(e =>
                    e.Status is EventStatus.Offen or EventStatus.Geschlossen or EventStatus.Abgesagt or EventStatus.Archiviert);
            }

            return Ok(filtered.Select(e => EventListMapper.ToDto(e, email)));
        }


        [HttpPut("subscribe/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> Subscribe(int eventId)
        {
            var accountIdStr = User.FindFirst("accountId")?.Value;
            if (!int.TryParse(accountIdStr, out var accountId))
                return Unauthorized("accountId claim fehlt.");

            var result = await _eventService.SubscribeToEvent(accountId, eventId);
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }

        [HttpPut("unsubscribe/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(int eventId)
        {
            var accountIdStr = User.FindFirst("accountId")?.Value;
            if (!int.TryParse(accountIdStr, out var accountId))
                return Unauthorized("accountId claim fehlt.");

            var result = await _eventService.UnsubscribeToEvent(accountId, eventId);
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }

        [HttpDelete("{eventId:int}/participant/{accountId:int}")]
        [Authorize(Roles = "Owner,Organisator")]
        public async Task<IActionResult> RemoveParticipant(int eventId, int accountId)
        {
            var result = await _eventService.UnsubscribeToEvent(accountId, eventId);
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto dto)
        {
            // Orga-Domain direkt aus dem JWT-Claim holen
            var orgDomain = User.FindFirst("org")?.Value;
            if (string.IsNullOrWhiteSpace(orgDomain))
                return Unauthorized("Kein gültiger Orga-Domain-Claim.");

            // Organisation aus DB holen
            var orgResult = await _organizationService.GetOrganization(orgDomain);
            if (orgResult.IsFailed)
                return BadRequest("Organisation nicht gefunden.");

            // An Service übergeben (Model + OrgaId)
            var result = await _eventService.CreateEvent(dto, orgResult.Value.Id, User);

            return result.IsSuccess
                ? Ok()
                : BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }
        
        [HttpPut("{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateEvent(int eventId, [FromBody] EventDto dto)
        {
            if (eventId != dto.Id)
                return BadRequest("IDs passen nicht zusammen.");

            var result = await _eventService.UpdateEventAsync(dto, User);
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }

        [HttpDelete("{eventId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            var role  = TokenHelper.GetRole(User);
            var email = TokenHelper.GetEmail(User);

            var evRes = await _eventService.GetEvent(eventId);
            if (evRes.IsFailed) return NotFound();

            var isOrganizer = evRes.Value.Organizers
                .Any(o => o.EMail.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (!(role == "Owner" || isOrganizer))
                return Forbid();

            var result = await _eventService.DeleteEvent(eventId);
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }

        [HttpGet("{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> GetEvent(int eventId)
        {
            var result = await _eventService.GetEvent(eventId);
            if (result.IsFailed)
                return NotFound();

            var dto = EventMapper.ToDto(result.Value);
            return Ok(dto);
        }
    }
}
