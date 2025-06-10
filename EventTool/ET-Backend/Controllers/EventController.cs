using System.Security.Claims;
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
        private readonly IAccountService _accountService;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventService eventService, IUserService userService,IAccountService accountService, IOrganizationService organizationService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _userService = userService;
            _accountService = accountService;
            _organizationService = organizationService;
            _logger = logger;
        }

        [HttpGet("eventList/{domain}")]
        [Authorize]
        public async Task<IActionResult> EventList(string domain)
        {
            // 1️⃣ Account-Id aus NameIdentifier-Claim ziehen
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                return Unauthorized();

            // Organisation holen
            var orgResult = await _organizationService.GetOrganization(domain);
            if (orgResult.IsFailed)
                return Unauthorized("Organisation nicht gefunden.");

            // Events holen
            var result = await _eventService.GetEventsFromOrganization(orgResult.Value.Id);

            if (result.IsSuccess)
            {
                // 2️⃣ Mapper bekommt jetzt die Account-Id
                var dtoList = result.Value
                    .Select(e => EventListMapper.ToDto(e, accountId))
                    .ToList();


                return Ok(dtoList);
            }

            return BadRequest(result.Errors);
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

        [HttpPost("createEvent")]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto value)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || user.Organization == null)
                return Unauthorized("Ungültiger Benutzer oder keine Organisation gefunden.");

            // Grunddaten
            var newEvent = new Event
            {
                Name = value.Name,
                Description = value.Description,
                Location = value.Location,
                StartDate = value.StartDate,
                EndDate = value.EndDate,
                StartTime = value.StartTime,
                EndTime = value.EndTime,
                MinParticipants = value.MinParticipants,
                MaxParticipants = value.MaxParticipants,
                RegistrationStart = value.RegistrationStart,
                RegistrationEnd = value.RegistrationEnd,
                IsBlueprint = value.IsBlueprint
            };

            // Organizers
            var organizerAccounts = new List<Account>();
            foreach (string organizer in value.Organizers.Distinct())
            {
                var accResult = await _accountService.GetAccountByMail(organizer);
                if (accResult.IsSuccess)
                    organizerAccounts.Add(accResult.Value);
            }
            newEvent.Organizers = organizerAccounts;

            // ContactPersons
            var contactAccounts = new List<Account>();
            foreach (string contact in value.ContactPersons.Distinct())
            {
                var accResult = await _accountService.GetAccountByMail(contact);
                if (accResult.IsSuccess)
                    contactAccounts.Add(accResult.Value);
            }
            newEvent.ContactPersons = contactAccounts;

            // Event anlegen
            var result = await _eventService.CreateEvent(newEvent, user.Organization.Id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Event erstellt: {Name} durch {User}", value.Name, user.EMail);
                return Ok(); // oder: Ok(EventDtoMapper.ToDto(result.Value));
            }

            // Fehler mit ausgeben
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
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

            var e = result.Value;

            var dto = new EventDto(
                e.Name,
                e.Description,
                e.Location,
                e.Organizers     .Select(o => o.EMail).ToList(),
                e.ContactPersons .Select(c => c.EMail).ToList(),
                e.Process?.Id ?? 0,
                e.StartDate,
                e.EndDate,
                e.StartTime,
                e.EndTime,
                e.MinParticipants,
                e.MaxParticipants,
                e.RegistrationStart,
                e.RegistrationEnd,
                e.IsBlueprint
            );

            return Ok(dto);
        }
    }
}
