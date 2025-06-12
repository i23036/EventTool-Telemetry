using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ET_Backend.Models;
using ET_Backend.Repository.Event;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using ET_Backend.Repository.Processes;
using ET_Backend.Services.Helper;
using ET_Backend.Services.Mapping;
using ET_Backend.Services.Organization;
using ET_Backend.Services.Person;
using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Services.Event;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IProcessRepository _processRepository;
    private readonly ILogger<EventService> _logger;
    private readonly IAccountService _accountService;

    public EventService(
        IEventRepository eventRepository,
        IOrganizationRepository organizationRepository,
        IAccountRepository accountRepository,
        IProcessRepository processRepository,
        ILogger<EventService> logger,
        IAccountService accountService)
    {
        _eventRepository = eventRepository;
        _organizationRepository = organizationRepository;
        _accountRepository = accountRepository;
        _processRepository = processRepository;
        _logger = logger;
        _accountService = accountService;
    }

    public async Task<Result<Models.Event>> CreateEvent(Models.Event newEvent, int organizationId)
    {
        // Organisatoren laden
        var organizerAccounts = new List<Account>();
        foreach (var email in newEvent.Organizers.Select(o => o.EMail).Distinct())
        {
            var accRes = await _accountRepository.GetAccount(email);
            if (accRes.IsFailed)
            {
                _logger.LogWarning("Kein Account für Organizer-E-Mail '{Email}' gefunden.", email);
                return Result.Fail($"Kein Account für Organizer-E-Mail '{email}' gefunden.");
            }
            organizerAccounts.Add(accRes.Value);
        }

        // Kontaktpersonen laden
        var contactPersonAccounts = new List<Account>();
        foreach (var email in newEvent.ContactPersons.Select(c => c.EMail).Distinct())
        {
            var accRes = await _accountRepository.GetAccount(email);
            if (accRes.IsFailed)
            {
                _logger.LogWarning("Kein Account für ContactPerson-E-Mail '{Email}' gefunden.", email);
                return Result.Fail($"Kein Account für ContactPerson-E-Mail '{email}' gefunden.");
            }
            contactPersonAccounts.Add(accRes.Value);
        }

        // Organisation laden
        var orgRes = await _organizationRepository.GetOrganization(organizationId);
        if (orgRes.IsFailed)
        {
            _logger.LogWarning("Organisation mit Id {OrgId} nicht gefunden.", organizationId);
            return Result.Fail("Organisation nicht gefunden.");
        }

        newEvent.Organization = orgRes.Value;
        newEvent.Organizers = organizerAccounts;
        newEvent.ContactPersons = contactPersonAccounts;

        // Event speichern
        var result = await _eventRepository.CreateEvent(newEvent, organizationId);

        if (result.IsSuccess)
            _logger.LogInformation("Event '{EventName}' erfolgreich erstellt (Id={EventId}).", newEvent.Name, result.Value.Id);
        else
            _logger.LogError("Fehler beim Erstellen des Events '{EventName}': {Error}", newEvent.Name, result.Errors.FirstOrDefault()?.Message);

        return result;
    }

    public async Task<Result> UpdateEventAsync(EventDto dto, ClaimsPrincipal user)
    {
        // Claims via TokenHelper auslesen
        var email     = TokenHelper.GetEmail(user);
        var role      = TokenHelper.GetRole(user);
        var orgDomain = TokenHelper.GetOrgDomain(user);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(orgDomain))
            return Result.Fail("Fehlende Benutzerinformationen im Token.");

        // Berechtigung prüfen
        bool isOwner     = role.Equals("Owner", StringComparison.OrdinalIgnoreCase);
        bool isOrganizer = dto.Organizers.Contains(email);

        if (!isOwner && !isOrganizer)
            return Result.Fail("Keine Berechtigung zur Bearbeitung dieses Events.");

        // Organisation laden
        var orgResult = await _organizationRepository.GetOrganization(orgDomain);
        if (orgResult.IsFailed)
            return Result.Fail("Organisation konnte nicht geladen werden.");

        // Mapping (DTO → Model)
        var ev = EventMapper.ToModel(dto, orgResult.Value);
        ev.Id = dto.Id;

        // Teilnehmer/Rollen auflösen (Accounts)
        var res = await _accountService.ResolveEmailsAsync(
            dto.Organizers,
            dto.ContactPersons,
            dto.Participants.Select(p => p.Email).ToList());

        if (res.IsFailed)
            return Result.Fail(res.Errors);

        ev.Organizers     = res.Value.Organizers;
        ev.ContactPersons = res.Value.ContactPersons;
        ev.Participants   = res.Value.Participants;

        // Speichern
        return await _eventRepository.EditEvent(ev);
    }

    public async Task<Result<List<Models.Event>>> GetEventsFromOrganization(int organizationId)
    {
        var result = await _eventRepository.GetEventsByOrganization(organizationId);
        if (result.IsSuccess)
            _logger.LogInformation("Events geladen für OrganizationId: {OrgId}", organizationId);
        else
            _logger.LogWarning("Fehler beim Laden der Events für OrganizationId: {OrgId}. Fehler: {Error}", organizationId, result.Errors.FirstOrDefault()?.Message);

        return result;
    }

    public async Task<Result<List<Models.Event>>> GetEventsFromOrganization(string domain)
    {
        var orgResult = await _organizationRepository.GetOrganization(domain);
        if (orgResult.IsFailed)
        {
            _logger.LogWarning("Organisation für Domain '{Domain}' nicht gefunden.", domain);
            return Result.Fail("Organisation nicht gefunden.");
        }

        _logger.LogInformation("Organisation gefunden für Domain '{Domain}': Id={OrgId}", domain, orgResult.Value.Id);
        return await GetEventsFromOrganization(orgResult.Value.Id);
    }

    public async Task<Result> SubscribeToEvent(int accountId, int eventId)
    {
        var result = await _eventRepository.AddParticipant(accountId, eventId);
        if (result.IsSuccess)
            _logger.LogInformation("Account {AccountId} hat sich zu Event {EventId} angemeldet.", accountId, eventId);
        else
            _logger.LogWarning("Account {AccountId} konnte sich nicht zu Event {EventId} anmelden. Fehler: {Error}", accountId, eventId, result.Errors.FirstOrDefault()?.Message);

        return result;
    }

    public async Task<Result> UnsubscribeToEvent(int accountId, int eventId)
    {
        var result = await _eventRepository.RemoveParticipant(accountId, eventId);
        if (result.IsSuccess)
            _logger.LogInformation("Account {AccountId} hat sich von Event {EventId} abgemeldet.", accountId, eventId);
        else
            _logger.LogWarning("Account {AccountId} konnte sich nicht von Event {EventId} abmelden. Fehler: {Error}", accountId, eventId, result.Errors.FirstOrDefault()?.Message);

        return result;
    }

    public async Task<Result> DeleteEvent(int eventId)
    {
        var result = await _eventRepository.DeleteEvent(eventId);
        if (result.IsSuccess)
            _logger.LogInformation("Event {EventId} erfolgreich gelöscht.", eventId);
        else
            _logger.LogWarning("Fehler beim Löschen von Event {EventId}: {Error}", eventId, result.Errors.FirstOrDefault()?.Message);

        return result;
    }

    public async Task<Result<Models.Event>> GetEvent(int eventId)
    {
        var result = await _eventRepository.GetEvent(eventId);
        if (result.IsSuccess)
            _logger.LogInformation("Event {EventId} erfolgreich geladen.", eventId);
        else
            _logger.LogWarning("Fehler beim Laden von Event {EventId}: {Error}", eventId, result.Errors.FirstOrDefault()?.Message);

        return result;
    }
}