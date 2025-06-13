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
            var role  = TokenHelper.GetRole(user);               // Owner | Organisator | Member

            // Events holen (alle)
            var orgRes = await _organizationService.GetOrganization(domain);
            if (orgRes.IsFailed) return Unauthorized();

            var evRes = await _eventService.GetEventsFromOrganization(orgRes.Value.Id);
            if (evRes.IsFailed) return BadRequest(evRes.Errors);

            var filtered = evRes.Value.Where(e =>
                    role == "Owner" ? true :                                 // Owner sieht alles
                        role == "Organisator"
                            ? (e.Status != EventStatus.Entwurf ||
                               e.Organizers.Any(o => o.EMail == email))          // sieht Entwürfe nur als Verwalter
                            : e.Status is EventStatus.Offen
                                or EventStatus.Geschlossen
                                or EventStatus.Abgesagt
                                or EventStatus.Archiviert             // Member-Sicht
            );

            return Ok(filtered.Select(e => EventListMapper.ToDto(e, email)));
        }


        [HttpPut("subscribe/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> Subscribe(int eventId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                return Unauthorized();

            var result = await _eventService.SubscribeToEvent(accountId, eventId);
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }

        [HttpPut("unsubscribe/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(int eventId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                return Unauthorized();

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
        public async Task<IActionResult> CreateEvent([FromBody] EventDto value)
        {
            // Orga-Domain direkt aus dem JWT-Claim holen
            var orgDomain = User.FindFirst("org")?.Value;
            if (string.IsNullOrWhiteSpace(orgDomain))
                return Unauthorized("Kein gültiger Orga-Domain-Claim.");

            // Organisation aus DB holen
            var orgResult = await _organizationService.GetOrganization(orgDomain);
            if (orgResult.IsFailed)
                return BadRequest("Organisation nicht gefunden.");

            // Mapper: DTO → Model
            var newEvent = EventMapper.ToModel(value, orgResult.Value);

            // An Service übergeben (Model + OrgaId)
            var result = await _eventService.CreateEvent(newEvent, orgResult.Value.Id, User);

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
