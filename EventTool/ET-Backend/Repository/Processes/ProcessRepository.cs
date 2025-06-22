using System.Data;
using Dapper;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Processes;

/// <summary>Dapper-Zugriffsschicht für Prozesse + Prozessschritte (event-basiert).</summary>
public class ProcessRepository : IProcessRepository
{
    private readonly IDbConnection _db;
    public ProcessRepository(IDbConnection db) => _db = db;

    // ─────────────────────── Lesen ───────────────────────
    public async Task<Result<Process>> GetByEvent(int eventId)
    {
        var proc = await _db.QuerySingleOrDefaultAsync<Process>(
            "SELECT Id, EventId FROM Processes WHERE EventId = @Evt;",
            new { Evt = eventId });

        if (proc == null)
            return Result.Fail("NotFound");

        proc.ProcessSteps = (await _db.QueryAsync<ProcessStep>(
                "SELECT * FROM ProcessSteps WHERE ProcessId = @Pid;",
                new { Pid = proc.Id }))
            .ToList();

        return Result.Ok(proc);
    }

    // ─────────────────────── Schreiben ───────────────────────
    public async Task<Result> Upsert(Process proc)
    {
        using var tx = _db.BeginTransaction();

        if (proc.Id == 0)
        {
            proc.Id = await _db.ExecuteScalarAsync<int>(
                "INSERT INTO Processes (EventId) VALUES (@Evt); SELECT last_insert_rowid();",
                new { Evt = proc.EventId }, tx);
        }

        // alte Steps entfernen
        await _db.ExecuteAsync("DELETE FROM ProcessSteps WHERE ProcessId = @Pid;", new { Pid = proc.Id }, tx);

        // ---------- 1) neu einfügen & ID-Mapping bauen ----------
        var idMap = new Dictionary<int, int>();   // alt -> neu
        foreach (var s in proc.ProcessSteps)
        {
            int newId = await _db.ExecuteScalarAsync<int>(@"
                INSERT INTO ProcessSteps
                    (Name, Trigger, Action, Offset,
                    TriggeredByStepId, Subject, Body, ProcessId)
                VALUES (@StepName, @Trig, @Act, @Off,
                       NULL, @Subj, @Bod, @Pid);
                SELECT last_insert_rowid();",
                new {
                    StepName = s.Name,
                    Trig     = (int)s.Trigger,
                    Act      = (int)s.Action,
                    Off      = s.Offset,
                    Subj     = s.Subject,
                    Bod      = s.Body,
                    Pid      = proc.Id
                }, tx);


            idMap[s.Id] = newId;          // negative/alte ID → neue ID
            s.Id = newId;                 // damit der Aufrufer die echten IDs bekommt
        }

        // ---------- 2) Verkettungen nachziehen ----------
        foreach (var s in proc.ProcessSteps.Where(p => p.TriggeredByStepId.HasValue))
        {
            int oldParentId = s.TriggeredByStepId!.Value;
            if (idMap.TryGetValue(oldParentId, out int newParentId))
            {
                await _db.ExecuteAsync(
                    "UPDATE ProcessSteps SET TriggeredByStepId = @NewPid WHERE Id = @Id;",
                    new { NewPid = newParentId, Id = s.Id }, tx);
            }
        }

        tx.Commit();
        return Result.Ok();
    }
}
