using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Event;

public class EventRepository : IEventRepository
{
    private readonly IDbConnection _db;

    public EventRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Result<bool>> EventExists(int eventId)
    {
        try
        {
            var exists = await _db.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Events WHERE Id = @Id",
                new { Id = eventId });
            return Result.Ok(exists);
        }
        catch
        {
            return Result.Fail("DBError");
        }
    }

    public async Task<Result<Models.Event>> CreateEvent(Models.Event newEvent, int organizationId)
    {
        using var transaction = _db.BeginTransaction();

        try
        {
            var insertSql = @"
            INSERT INTO Events (
                Name, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime,
                Location, MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, IsBlueprint
            ) VALUES (
                @Name, @Description, @OrganizationId, @ProcessId,
                @StartDate, @EndDate, @StartTime, @EndTime,
                @Location, @MinParticipants, @MaxParticipants,
                @RegistrationStart, @RegistrationEnd, @IsBlueprint
            );";

            var eventId = await _db.InsertAndGetIdAsync(insertSql, new
            {
                newEvent.Name,
                newEvent.Description,
                OrganizationId = organizationId,
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
            }, transaction);

            // EventMembers einfügen (Participants, Organizers, ContactPersons)
            async Task InsertEventMembers(List<Account> accounts, bool isOrganizer, bool isContactPerson)
            {
                foreach (var account in accounts)
                {
                    await _db.ExecuteAsync(@"
                    INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
                    VALUES (@AccountId, @EventId, @IsOrganizer, @IsContactPerson)
                    ON CONFLICT(AccountId, EventId) DO NOTHING;",
                        new
                        {
                            AccountId = account.Id,
                            EventId = eventId,
                            IsOrganizer = isOrganizer ? 1 : 0,
                            IsContactPerson = isContactPerson ? 1 : 0
                        }, transaction);
                }
            }

            if (newEvent.Participants?.Any() == true)
                await InsertEventMembers(newEvent.Participants, isOrganizer: false, isContactPerson: false);

            if (newEvent.Organizers?.Any() == true)
                await InsertEventMembers(newEvent.Organizers, isOrganizer: true, isContactPerson: false);

            if (newEvent.ContactPersons?.Any() == true)
                await InsertEventMembers(newEvent.ContactPersons, isOrganizer: false, isContactPerson: true);

            transaction.Commit();

            return await GetEvent(eventId);
        }
        catch
        {
            transaction.Rollback();
            return Result.Fail("DBError");
        }
    }



    public async Task<Result> DeleteEvent(int eventId)
    {
        using var transaction = _db.BeginTransaction();

        try
        {
            // 1. Lösche alle Verknüpfungen in EventMembers
            await _db.ExecuteAsync(
                "DELETE FROM EventMembers WHERE EventId = @Id",
                new { Id = eventId }, transaction);

            // 2. Lösche das Event selbst
            var affected = await _db.ExecuteAsync(
                "DELETE FROM Events WHERE Id = @Id",
                new { Id = eventId }, transaction);

            transaction.Commit();

            return affected > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch
        {
            transaction.Rollback();
            return Result.Fail("DBError");
        }
    }


    public async Task<Result<Models.Event>> GetEvent(int eventId)
    {
        try
        {
            var evt = await _db.QueryFirstOrDefaultAsync<Models.Event>(
                @"SELECT 
                Id, Name, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime,
                Location, MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, IsBlueprint
              FROM Events
              WHERE Id = @Id",
                new { Id = eventId });

            if (evt == null)
                return Result.Fail("NotFound");

            // Organization laden
            evt.Organization = await _db.QueryFirstOrDefaultAsync<Models.Organization>(
                @"SELECT Id, Name, Domain, Description, OrgaPicAsBase64 
              FROM Organizations 
              WHERE Id = @Id",
                new { Id = evt.Organization?.Id ?? 0 });

            // Process laden (optional)
            if (evt.Process?.Id > 0)
            {
                evt.Process = await _db.QueryFirstOrDefaultAsync<Models.Process>(
                    @"SELECT Id, Name, OrganizationId 
                  FROM Processes 
                  WHERE Id = @Id",
                    new { Id = evt.Process.Id });
            }

            // Teilnehmerdaten laden (Accounts ohne User)
            var memberRows = await _db.QueryAsync<dynamic>(
                @"SELECT 
                em.IsOrganizer, em.IsContactPerson,
                a.Id as AccountId, a.Email, a.IsVerified
              FROM EventMembers em
              JOIN Accounts a ON em.AccountId = a.Id
              WHERE em.EventId = @EventId",
                new { EventId = evt.Id });

            foreach (var row in memberRows)
            {
                var account = new Models.Account
                {
                    Id = row.AccountId,
                    EMail = row.Email,
                    IsVerified = row.IsVerified
                };

                evt.Participants.Add(account);

                if (row.IsOrganizer == 1)
                    evt.Organizers.Add(account);

                if (row.IsContactPerson == 1)
                    evt.ContactPersons.Add(account);
            }

            return Result.Ok(evt);
        }
        catch
        {
            return Result.Fail("DBError");
        }
    }


    public async Task<Result<List<Models.Event>>> GetEventsByOrganization(int organizationId)
    {
        try
        {
            var sqlEvents = $@"
            SELECT 
                Id, Name, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime,
                Location, MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, IsBlueprint
            FROM {_db.Tbl("Events")}
            WHERE OrganizationId = @OrgId";

            var events = (await _db.QueryAsync<Models.Event>(sqlEvents, new { OrgId = organizationId }))
                         .ToList();

            if (!events.Any())
                return Result.Ok(events);

            var eventIds = events.Select(e => e.Id).ToList();

            // Nur Account-Daten laden (kein User mehr)
            var sqlMembers = $@"
            SELECT 
                em.EventId,
                a.Id as AccountId, a.Email, a.IsVerified,
                em.IsOrganizer, em.IsContactPerson
            FROM {_db.Tbl("EventMembers")} em
            JOIN {_db.Tbl("Accounts")} a ON em.AccountId = a.Id
            WHERE em.EventId IN @EventIds";

            var eventMemberRows = await _db.QueryAsync<dynamic>(sqlMembers, new { EventIds = eventIds });

            var eventMemberMap = new Dictionary<int, List<(Models.Account Account, bool IsOrganizer, bool IsContact)>>();

            foreach (var row in eventMemberRows)
            {
                var account = new Models.Account
                {
                    Id = row.AccountId,
                    EMail = row.Email,
                    IsVerified = row.IsVerified
                };

                int eventId = row.EventId;

                if (!eventMemberMap.ContainsKey(eventId))
                    eventMemberMap[eventId] = new List<(Models.Account, bool, bool)>();

                eventMemberMap[eventId].Add((account, row.IsOrganizer == 1, row.IsContactPerson == 1));
            }

            // Organisationen und Prozesse laden
            var orgIds = events.Select(e => e.Organization?.Id ?? 0).Distinct().ToList();
            var processIds = events.Select(e => e.Process?.Id ?? 0).Where(id => id > 0).Distinct().ToList();

            var orgs = (await _db.QueryAsync<Models.Organization>(
                $"SELECT Id, Name, Domain, Description, OrgaPicAsBase64 FROM {_db.Tbl("Organizations")} WHERE Id IN @Ids",
                new { Ids = orgIds })).ToDictionary(o => o.Id);

            var processes = (await _db.QueryAsync<Models.Process>(
                $"SELECT Id, Name, OrganizationId FROM {_db.Tbl("Processes")} WHERE Id IN @Ids",
                new { Ids = processIds })).ToDictionary(p => p.Id);

            // Events zusammensetzen
            foreach (var e in events)
            {
                e.Participants = new();
                e.Organizers = new();
                e.ContactPersons = new();

                if (eventMemberMap.TryGetValue(e.Id, out var members))
                {
                    foreach (var (account, isOrg, isContact) in members)
                    {
                        e.Participants.Add(account);

                        if (isOrg)
                            e.Organizers.Add(account);

                        if (isContact)
                            e.ContactPersons.Add(account);
                    }
                }

                if (orgs.TryGetValue(e.Organization?.Id ?? 0, out var org))
                    e.Organization = org;

                if (processes.TryGetValue(e.Process?.Id ?? 0, out var proc))
                    e.Process = proc;
            }

            return Result.Ok(events);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }



    public async Task<Result> EditEvent(Models.Event currentEvent)
    {
        using var transaction = _db.BeginTransaction();

        try
        {
            var affected = await _db.ExecuteAsync(@"
            UPDATE Events SET
                Name = @Name,
                Description = @Description,
                OrganizationId = @OrganizationId,
                ProcessId = @ProcessId,
                StartDate = @StartDate,
                EndDate = @EndDate,
                StartTime = @StartTime,
                EndTime = @EndTime,
                Location = @Location,
                MinParticipants = @MinParticipants,
                MaxParticipants = @MaxParticipants,
                RegistrationStart = @RegistrationStart,
                RegistrationEnd = @RegistrationEnd,
                IsBlueprint = @IsBlueprint
            WHERE Id = @Id;",
                new
                {
                    currentEvent.Name,
                    currentEvent.Description,
                    OrganizationId = currentEvent.Organization?.Id,
                    ProcessId = currentEvent.Process?.Id,
                    currentEvent.StartDate,
                    currentEvent.EndDate,
                    currentEvent.StartTime,
                    currentEvent.EndTime,
                    currentEvent.Location,
                    currentEvent.MinParticipants,
                    currentEvent.MaxParticipants,
                    currentEvent.RegistrationStart,
                    currentEvent.RegistrationEnd,
                    IsBlueprint = currentEvent.IsBlueprint ? 1 : 0,
                    currentEvent.Id
                }, transaction);

            // EventMembers löschen
            await _db.ExecuteAsync("DELETE FROM EventMembers WHERE EventId = @EventId;",
                new { EventId = currentEvent.Id }, transaction);

            // Teilnehmer ohne besondere Rollen
            foreach (var participant in currentEvent.Participants.DistinctBy(a => a.Id))
            {
                await _db.ExecuteAsync(@"
                INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
                VALUES (@AccountId, @EventId, 0, 0);",
                    new { AccountId = participant.Id, EventId = currentEvent.Id }, transaction);
            }

            // Organisatoren
            foreach (var organizer in currentEvent.Organizers.DistinctBy(a => a.Id))
            {
                await _db.ExecuteAsync(@"
                INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
                VALUES (@AccountId, @EventId, 1, 0)
                ON CONFLICT(AccountId, EventId) DO UPDATE SET IsOrganizer = 1;",
                    new { AccountId = organizer.Id, EventId = currentEvent.Id }, transaction);
            }

            // Kontaktpersonen
            foreach (var contact in currentEvent.ContactPersons.DistinctBy(a => a.Id))
            {
                await _db.ExecuteAsync(@"
                INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
                VALUES (@AccountId, @EventId, 0, 1)
                ON CONFLICT(AccountId, EventId) DO UPDATE SET IsContactPerson = 1;",
                    new { AccountId = contact.Id, EventId = currentEvent.Id }, transaction);
            }

            transaction.Commit();
            return affected > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch
        {
            transaction.Rollback();
            return Result.Fail("DBError");
        }
    }

}
