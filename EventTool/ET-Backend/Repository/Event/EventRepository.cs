using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Event;

public class EventRepository : IEventRepository
{
    private readonly IDbConnection _db;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(IDbConnection db, ILogger<EventRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    private async Task UpsertEventMember(int accId, int evtId, bool isOrg, bool isContact, IDbTransaction? tx = null)
    {
        try
        {
            if (_db.IsSQLite())
            {
                await _db.ExecuteAsync(@"
                INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
                VALUES (@Acc, @Evt, @IsOrg, @IsContact)
                ON CONFLICT(AccountId, EventId) DO UPDATE
                SET IsOrganizer     = CASE WHEN @IsOrg    = 1 THEN 1 ELSE IsOrganizer     END,
                    IsContactPerson = CASE WHEN @IsContact = 1 THEN 1 ELSE IsContactPerson END;",
                    new { Acc = accId, Evt = evtId, IsOrg = isOrg ? 1 : 0, IsContact = isContact ? 1 : 0 }, tx);
            }
            else
            {
                await _db.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.EventMembers WHERE AccountId=@Acc AND EventId=@Evt)
                    INSERT INTO dbo.EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
                    VALUES (@Acc, @Evt, @IsOrg, @IsContact);
                ELSE
                    UPDATE dbo.EventMembers
                    SET IsOrganizer     = CASE WHEN @IsOrg    = 1 THEN 1 ELSE IsOrganizer     END,
                        IsContactPerson = CASE WHEN @IsContact = 1 THEN 1 ELSE IsContactPerson END
                    WHERE AccountId=@Acc AND EventId=@Evt;",
                    new { Acc = accId, Evt = evtId, IsOrg = isOrg ? 1 : 0, IsContact = isContact ? 1 : 0 }, tx);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei UpsertEventMember: AccId={AccId}, EventId={EventId}", accId, evtId);
            throw;
        }
    }

    public async Task<Result<bool>> EventExists(int eventId)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<long>($@"
            SELECT COUNT(1)
            FROM {_db.Tbl("Events")}
            WHERE Id=@Id;",
                new { Id = eventId });

            return Result.Ok(exists > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Prüfen, ob Event existiert: {EventId}", eventId);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Models.Event>> CreateEvent(Models.Event newEvent, int orgId)
    {
        using var tx = _db.BeginTransaction();
        try
        {
            var insertSql = $@"
            INSERT INTO {_db.Tbl("Events")} (
                Name, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime,
                Location, MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, IsBlueprint)
            VALUES (
                @Name, @Description, @OrganizationId, @ProcessId,
                @StartDate, @EndDate, @StartTime, @EndTime,
                @Location, @MinParticipants, @MaxParticipants,
                @RegistrationStart, @RegistrationEnd, @IsBlueprint);";

            var evtId = await _db.InsertAndGetIdAsync(insertSql, new
            {
                newEvent.Name,
                newEvent.Description,
                OrganizationId = orgId,
                ProcessId = newEvent.Process?.Id,
                newEvent.StartDate,
                newEvent.EndDate,
                newEvent.StartTime,
                newEvent.EndTime,
                newEvent.Location,
                newEvent.MinParticipants,
                newEvent.MaxParticipants,
                newEvent.RegistrationStart,
                newEvent.RegistrationEnd,
                IsBlueprint = newEvent.IsBlueprint ? 1 : 0
            }, tx);

            if (newEvent.Participants?.Any() == true)
                foreach (var p in newEvent.Participants)
                    await UpsertEventMember(p.Id, evtId, false, false, tx);

            if (newEvent.Organizers?.Any() == true)
                foreach (var o in newEvent.Organizers)
                    await UpsertEventMember(o.Id, evtId, true, false, tx);

            if (newEvent.ContactPersons?.Any() == true)
                foreach (var c in newEvent.ContactPersons)
                    await UpsertEventMember(c.Id, evtId, false, true, tx);

            tx.Commit();
            _logger.LogInformation("Event erstellt: {EventId}", evtId);
            return await GetEvent(evtId);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Fehler beim Erstellen eines Events");
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> DeleteEvent(int eventId)
    {
        using var tx = _db.BeginTransaction();
        try
        {
            await _db.ExecuteAsync($@"
            DELETE FROM {_db.Tbl("EventMembers")}
            WHERE EventId=@Id;",
                new { Id = eventId }, tx);

            var affected = await _db.ExecuteAsync($@"
            DELETE FROM {_db.Tbl("Events")}
            WHERE Id=@Id;",
                new { Id = eventId }, tx);

            tx.Commit();
            _logger.LogInformation("Event gelöscht: {EventId}", eventId);
            return affected > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Fehler beim Löschen von Event: {EventId}", eventId);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Models.Event>> GetEvent(int eventId)
    {
        try
        {
            var evt = await _db.QueryFirstOrDefaultAsync<Models.Event>($@"
            SELECT Id, Name, Description, OrganizationId, ProcessId,
                   StartDate, EndDate, StartTime, EndTime,
                   Location, MinParticipants, MaxParticipants,
                   RegistrationStart, RegistrationEnd, IsBlueprint
            FROM {_db.Tbl("Events")}
            WHERE Id=@Id;",
                new { Id = eventId });

            if (evt == null) return Result.Fail("NotFound");

            // Init-Listen, falls null
            evt.Participants ??= new();
            evt.Organizers ??= new();
            evt.ContactPersons ??= new();

            // Orga laden
            evt.Organization = await _db.QueryFirstOrDefaultAsync<Models.Organization>($@"
            SELECT Id, Name, Domain, Description, OrgaPicAsBase64
            FROM {_db.Tbl("Organizations")}
            WHERE Id=@Id;",
                new { Id = evt.Organization?.Id ?? 0 });

            // Process laden (optional)
            if (evt.Process?.Id > 0)
            {
                evt.Process = await _db.QueryFirstOrDefaultAsync<Process>($@"
                SELECT Id, Name, OrganizationId
                FROM {_db.Tbl("Processes")}
                WHERE Id=@Id;",
                    new { Id = evt.Process.Id });
            }

            // Teilnehmer
            var rows = await _db.QueryAsync<dynamic>($@"
            SELECT a.Id AS AccId, a.Email, a.IsVerified,
                   em.IsOrganizer, em.IsContactPerson
            FROM {_db.Tbl("EventMembers")} em
            JOIN {_db.Tbl("Accounts")}     a ON em.AccountId = a.Id
            WHERE em.EventId = @Evt;",
                new { Evt = evt.Id });

            foreach (var r in rows)
            {
                var acc = new Account
                {
                    Id = Convert.ToInt32(r.AccId),
                    EMail = r.Email,
                    IsVerified = Convert.ToInt32(r.IsVerified) == 1
                };

                evt.Participants.Add(acc);
                if (Convert.ToInt32(r.IsOrganizer) == 1) evt.Organizers.Add(acc);
                if (Convert.ToInt32(r.IsContactPerson) == 1) evt.ContactPersons.Add(acc);
            }

            _logger.LogInformation("Event geladen: {EventId}", eventId);
            return Result.Ok(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen von Event {EventId}", eventId);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<List<Models.Event>>> GetEventsByOrganization(int orgId)
    {
        try
        {
            var events = (await _db.QueryAsync<Models.Event>($@"
            SELECT Id, Name, Description, OrganizationId, ProcessId,
                   StartDate, EndDate, StartTime, EndTime,
                   Location, MinParticipants, MaxParticipants,
                   RegistrationStart, RegistrationEnd, IsBlueprint
            FROM {_db.Tbl("Events")}
            WHERE OrganizationId=@Org;",
                new { Org = orgId })).ToList();

            if (!events.Any()) return Result.Ok(events);

            var ids = events.Select(e => e.Id).ToList();

            var rows = await _db.QueryAsync<dynamic>($@"
            SELECT em.EventId,
                   a.Id AS AccId, a.Email, a.IsVerified,
                   em.IsOrganizer, em.IsContactPerson
            FROM {_db.Tbl("EventMembers")} em
            JOIN {_db.Tbl("Accounts")}     a ON em.AccountId = a.Id
            WHERE em.EventId IN @Ids;",
                new { Ids = ids });

            var map = new Dictionary<int, List<(Account acc, bool org, bool cp)>>();

            foreach (var r in rows)
            {
                var acc = new Account
                {
                    Id = Convert.ToInt32(r.AccId),
                    EMail = r.Email,
                    IsVerified = Convert.ToInt32(r.IsVerified) == 1
                };

                int eid = Convert.ToInt32(r.EventId);

                if (!map.ContainsKey(eid)) map[eid] = new();
                map[eid].Add((acc,
                              Convert.ToInt32(r.IsOrganizer) == 1,
                              Convert.ToInt32(r.IsContactPerson) == 1));
            }

            var orgIds = events.Select(e => e.Organization?.Id ?? 0).Distinct();
            var processIds = events.Select(e => e.Process?.Id ?? 0).Where(i => i > 0).Distinct();

            var orgs = (await _db.QueryAsync<Models.Organization>($@"
            SELECT Id, Name, Domain, Description, OrgaPicAsBase64
            FROM {_db.Tbl("Organizations")}
            WHERE Id IN @Ids;",
                new { Ids = orgIds })).ToDictionary(o => o.Id);

            var procs = (await _db.QueryAsync<Process>($@"
            SELECT Id, Name, OrganizationId
            FROM {_db.Tbl("Processes")}
            WHERE Id IN @Ids;",
                new { Ids = processIds })).ToDictionary(p => p.Id);

            foreach (var e in events)
            {
                e.Participants = new();
                e.Organizers = new();
                e.ContactPersons = new();

                if (map.TryGetValue(e.Id, out var lst))
                    foreach (var (acc, org, cp) in lst)
                    {
                        e.Participants.Add(acc);
                        if (org) e.Organizers.Add(acc);
                        if (cp) e.ContactPersons.Add(acc);
                    }

                if (orgs.TryGetValue(e.Organization?.Id ?? 0, out var o)) e.Organization = o;
                if (procs.TryGetValue(e.Process?.Id ?? 0, out var p)) e.Process = p;
            }

            _logger.LogInformation("Events geladen für OrganizationId: {OrgId}", orgId);
            return Result.Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Events für OrganizationId: {OrgId}", orgId);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> EditEvent(Models.Event ev)
    {
        using var tx = _db.BeginTransaction();
        try
        {
            await _db.ExecuteAsync($@"
            UPDATE {_db.Tbl("Events")} SET
                Name=@Name, Description=@Description, OrganizationId=@OrgId,
                ProcessId=@ProcId, StartDate=@StartDate, EndDate=@EndDate,
                StartTime=@StartTime, EndTime=@EndTime, Location=@Location,
                MinParticipants=@MinPart, MaxParticipants=@MaxPart,
                RegistrationStart=@RegStart, RegistrationEnd=@RegEnd,
                IsBlueprint=@IsBp
            WHERE Id=@Id;",
                new
                {
                    ev.Name,
                    ev.Description,
                    OrgId = ev.Organization?.Id,
                    ProcId = ev.Process?.Id,
                    ev.StartDate,
                    ev.EndDate,
                    ev.StartTime,
                    ev.EndTime,
                    ev.Location,
                    MinPart = ev.MinParticipants,
                    MaxPart = ev.MaxParticipants,
                    RegStart = ev.RegistrationStart,
                    RegEnd = ev.RegistrationEnd,
                    IsBp = ev.IsBlueprint ? 1 : 0,
                    ev.Id
                }, tx);

            await _db.ExecuteAsync($@"
            DELETE FROM {_db.Tbl("EventMembers")}
            WHERE EventId=@Evt;",
                new { Evt = ev.Id }, tx);

            foreach (var p in ev.Participants.DistinctBy(a => a.Id))
                await UpsertEventMember(p.Id, ev.Id, false, false, tx);

            foreach (var o in ev.Organizers.DistinctBy(a => a.Id))
                await UpsertEventMember(o.Id, ev.Id, true, false, tx);

            foreach (var c in ev.ContactPersons.DistinctBy(a => a.Id))
                await UpsertEventMember(c.Id, ev.Id, false, true, tx);

            tx.Commit();
            _logger.LogInformation("Event bearbeitet: {EventId}", ev.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Fehler beim Bearbeiten von Event {EventId}", ev.Id);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }


    public async Task<Result> AddParticipant(int accountId, int eventId)
    {
        try
        {
            await UpsertEventMember(accountId, eventId, false, false);
            _logger.LogInformation("Teilnehmer hinzugefügt: AccountId={AccountId}, EventId={EventId}", accountId, eventId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Hinzufügen von Teilnehmer: AccountId={AccountId}, EventId={EventId}", accountId, eventId);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> RemoveParticipant(int accountId, int eventId)
    {
        try
        {
            await _db.ExecuteAsync($@"
            DELETE FROM {_db.Tbl("EventMembers")}
            WHERE AccountId=@Acc AND EventId=@Evt;",
                new { Acc = accountId, Evt = eventId });

            _logger.LogInformation("Teilnehmer entfernt: AccountId={AccountId}, EventId={EventId}", accountId, eventId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Entfernen von Teilnehmer: AccountId={AccountId}, EventId={EventId}", accountId, eventId);
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
}
