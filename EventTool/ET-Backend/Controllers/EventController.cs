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
        private readonly IAccountService _accountService;

        public EventController(IEventService eventService, IUserService userService, IAccountService accountService)
        {
            _eventService = eventService;
            _userService = userService;
            _accountService = accountService;
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
