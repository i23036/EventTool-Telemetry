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
                IsBlueprint = value.IsBlueprint,
                // TODO Process = await processRepo.GetByIdAsync(value.ProcessId),
            };

            var accounts1 = new List<Account>();
            foreach (string organizer in value.Organizers.Distinct())
            {
                var account = await _accountService.GetAccountByMail(organizer);
                if (account.IsSuccess)
                {
                    accounts1.Add(account.Value);
                }
            }
            newEvent.Organizers = accounts1;

            var accounts2 = new List<Account>();
            foreach (string contact in value.ContactPersons.Distinct())
            {
                var account = await _accountService.GetAccountByMail(contact);
                if (account.IsSuccess)
                {
                    accounts2.Add(account.Value);
                }
            }
            newEvent.Organizers = accounts2;

            Result<Event> result = await _eventService.CreateEvent(newEvent, user.Organization.Id);

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
