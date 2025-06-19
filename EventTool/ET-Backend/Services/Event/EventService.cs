using System.Security.Claims;
using ET_Backend.Models;
using ET_Backend.Repository.Event;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using ET_Backend.Repository.Processes;
using ET_Backend.Services.Helper;
using ET_Backend.Services.Mapping;
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

    // EventService.cs
public async Task<Result<Models.Event>> CreateEvent(
    EventDto dto, int organizationId, ClaimsPrincipal user)
{
    // ───────── 1. Creator-Infos  ─────────
    var creatorEmail = TokenHelper.GetEmail(user);
    var creatorRole  = TokenHelper.GetRole (user);

    if (string.IsNullOrWhiteSpace(creatorEmail) ||
        string.IsNullOrWhiteSpace(creatorRole))
        return Result.Fail("Benutzerinformationen im Token fehlen.");

    var isOwner     = creatorRole.Equals("Owner",     StringComparison.OrdinalIgnoreCase);
    var isOrganizer = creatorRole.Equals("Organizer", StringComparison.OrdinalIgnoreCase);

    // ───────── 2. Pflicht-Check  ─────────
    var selfListed = dto.Organizers
        .Any(m => m.Equals(creatorEmail, StringComparison.OrdinalIgnoreCase));

    if (isOrganizer && !selfListed)
        return Result.Fail("Als Organisator musst du dich selbst als Verwalter eintragen.");

    // ───────── 3. Accounts auflösen  ─────────
    var resolve = await _accountService.ResolveEmailsAsync(
                      dto.Organizers,
                      dto.ContactPersons,
                      dto.Participants.Select(p => p.Email).ToList());

    if (resolve.IsFailed) return Result.Fail(resolve.Errors);

    // ───────── 4. Organisation  ─────────
    var orgRes = await _organizationRepository.GetOrganization(organizationId);
    if (orgRes.IsFailed) return Result.Fail("Organisation nicht gefunden.");

    // ───────── 5. DTO → Model  ─────────
    var model = EventMapper.ToModel(
        dto,
        orgRes.Value,
        organizers:     resolve.Value.Organizers,
        contactPersons: resolve.Value.ContactPersons,
        participants:   resolve.Value.Participants);

    // ───────── 6. Speichern  ─────────
    var repoRes = await _eventRepository.CreateEvent(model, organizationId);

    _logger.LogInformation("CreateEvent: {Name} durch {User} ({Role}) – Erfolg={Ok}",
                           dto.Name, creatorEmail, creatorRole, repoRes.IsSuccess);

    return repoRes;
}
    
    public async Task<Result> UpdateEventAsync(EventDto dto, ClaimsPrincipal user)
    {
        // Claims auslesen
        var email     = TokenHelper.GetEmail(user);
        var role      = TokenHelper.GetRole(user);
        var orgDomain = TokenHelper.GetOrgDomain(user);

        if (string.IsNullOrWhiteSpace(email) || 
            string.IsNullOrWhiteSpace(role)  || 
            string.IsNullOrWhiteSpace(orgDomain))
            return Result.Fail("Fehlende Benutzerinformationen im Token.");

        var isOwner     = role.Equals("Owner", StringComparison.OrdinalIgnoreCase);
        
        // Aktuellen Stand des Events laden (wichtig für Rechte-Check)
        var currentEvRes = await _eventRepository.GetEvent(dto.Id);
        if (currentEvRes.IsFailed)
            return Result.Fail("Event nicht gefunden.");

        var wasOrganizer = currentEvRes.Value.Organizers
            .Any(o => o.EMail.Equals(email, StringComparison.OrdinalIgnoreCase));

        /*  ▸ Owner dürfen immer
            ▸ Ein (ehemaliger) Organizer darf, solange er vor der Änderung Organizer war.
               Dabei darf er sich aber selbst aus der neuen Liste entfernen, um die
               Verwaltung zu übergeben. */
        if (!isOwner && !wasOrganizer)
            return Result.Fail("Keine Berechtigung zur Bearbeitung dieses Events.");

        // Validierung: mindestens ein Organizer muss übrig bleiben
        if (dto.Organizers == null || dto.Organizers.Count == 0)
            return Result.Fail("Mindestens ein Verwalter muss ausgewählt sein.");
        
        // Organisation laden
        var orgResult = await _organizationRepository.GetOrganization(orgDomain);
        if (orgResult.IsFailed)
            return Result.Fail("Organisation konnte nicht geladen werden.");

        // Teilnehmer/Rollen auflösen (Accounts)
        var res = await _accountService.ResolveEmailsAsync(
            dto.Organizers,
            dto.ContactPersons,
            dto.Participants.Select(p => p.Email).ToList());

        if (res.IsFailed)
            return Result.Fail(res.Errors);

        // Mapping (DTO → Model) mit korrekten Accounts
        var ev = EventMapper.ToModel(
            dto,
            orgResult.Value,
            organizers:     res.Value.Organizers,
            contactPersons: res.Value.ContactPersons,
            participants:   res.Value.Participants
        );
        ev.Id = dto.Id;

        // Speichern
        var result = await _eventRepository.EditEvent(ev);

        _logger.LogInformation(
            "UpdateEvent: EventId={EventId} durch {User} ({Role}) – Erfolg={Success}",
            dto.Id, email, role, result.IsSuccess);
        
        return result;
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