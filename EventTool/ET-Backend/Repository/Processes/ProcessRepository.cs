using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;


namespace ET_Backend.Repository.Processes;

/// <summary>Dapper-Zugriffsschicht für Prozesse + Schritte (event-basiert).</summary>
public class ProcessRepository : IProcessRepository
{
    private readonly IDbConnection _db;
    public ProcessRepository(IDbConnection db) => _db = db;

    // ─────────── Lesen ───────────
    public async Task<Result<Process>> GetByEvent(int eventId)
    {
        var proc = await _db.QuerySingleOrDefaultAsync<Process>(
            "SELECT Id, EventId FROM Processes WHERE EventId=@Evt;",
            new { Evt = eventId });

        if (proc == null) return Result.Fail("NotFound");

        proc.ProcessSteps = (await _db.QueryAsync<ProcessStep>(
            "SELECT * FROM ProcessSteps WHERE ProcessId=@Pid;",
            new { Pid = proc.Id })).ToList();

        return Result.Ok(proc);
    }

    // ─────────── Schreiben ───────────
    public async Task<Result> Upsert(Process proc)
    {
        using var tx = _db.BeginTransaction();

        // 1) alte Steps löschen
        await _db.ExecuteAsync(
            "DELETE FROM ProcessSteps WHERE ProcessId=@Pid;",
            new { Pid = proc.Id }, tx);

        // 2) neue Steps einfügen
        foreach (var s in proc.ProcessSteps)
        {
            await _db.ExecuteAsync(@"
                INSERT INTO ProcessSteps
                      (TypeName, Type, Trigger, Condition, OffsetInHours, ProcessId)
                VALUES (@Name, @Type, @Trigger, @Cond, @Off, @Pid);",
                new
                {
                    Name    = s.TypeName,
                    Type    = (int)s.Type,
                    Trigger = (int)s.Trigger,
                    Cond    = (int)s.Condition,
                    Off     = s.OffsetInHours,
                    Pid     = proc.Id
                }, tx);
        }

        tx.Commit();
        return Result.Ok();
    }
}
