using System.Data;
using Dapper;
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

    public async Task<Result<Models.Event>> CreateEvent(string name, Models.Organization organization)
    {
        try
        {
            var insertSql = @"
            INSERT INTO " + _db.Tbl("Events") + @" (
                Name, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime,
                Location, MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, IsBlueprint
            ) VALUES (
                @Name, '', @OrganizationId, NULL,
                @StartDate, @EndDate, @StartTime, @EndTime,
                '', 0, 0, @RegistrationStart, @RegistrationEnd, 0
            )";

            var eventId = await _db.InsertAndGetIdAsync(insertSql, new
            {
                Name = name,
                OrganizationId = organization.Id,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.FromDateTime(DateTime.Now),
                EndTime = TimeOnly.FromDateTime(DateTime.Now),
                RegistrationStart = DateOnly.FromDateTime(DateTime.Today),
                RegistrationEnd = DateOnly.FromDateTime(DateTime.Today)
            });

            return await GetEvent(eventId);
        }
        catch
        {
            return Result.Fail("DBError");
        }
    }


    public async Task<Result> DeleteEvent(int eventId)
    {
        try
        {
            var affected = await _db.ExecuteAsync(
                "DELETE FROM Events WHERE Id = @Id",
                new { Id = eventId });

            return affected > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch
        {
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

            // Optional: Lade zugehörige Organization und Process, falls benötigt
            // evt.Organization = await _db.QueryFirstOrDefaultAsync<Organization>(
            //     "SELECT * FROM Organizations WHERE Id = @Id", new { Id = evt.OrganizationId });
            // evt.Process = await _db.QueryFirstOrDefaultAsync<Process>(
            //     "SELECT * FROM Processes WHERE Id = @Id", new { Id = evt.ProcessId });

            return Result.Ok(evt);
        }
        catch
        {
            return Result.Fail("DBError");
        }
    }

    public async Task<Result<List<Models.Event>>> GetEventsByOrganizationId(int organizationId)
    {
        try
        {
            var sql = $@"
            SELECT 
                Id, Name, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime,
                Location, MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, IsBlueprint
            FROM {_db.Tbl("Events")}
            WHERE OrganizationId = @OrgId";

            var events = await _db.QueryAsync<Models.Event>(sql, new { OrgId = organizationId });

            return Result.Ok(events.ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result> EditEvent(Models.Event currentEvent)
    {
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
                });

            return affected > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch
        {
            return Result.Fail("DBError");
        }
    }
}
