using Dapper;
using ET.Shared.DTOs.Enums;
using ET_Backend.Models.Enums;
using System.Data;
using Microsoft.Data.Sqlite;

namespace ET_Backend.Services.Processes;

/// <summary>
/// Prüft alle 30 Sekunden, ob ein Event seine Max-Teilnehmerzahl erreicht hat,
/// und setzt den Status von „Offen“ auf „Geschlossen“.
/// </summary>
public class ProcessWorkerService : BackgroundService
{
    private readonly IDbConnection _db;
    private readonly ILogger<ProcessWorkerService> _log;

    public ProcessWorkerService(IDbConnection db, ILogger<ProcessWorkerService> log)
    {
        _db  = db;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        while (!stop.IsCancellationRequested)
        {
            await EvaluateAsync();
            await Task.Delay(TimeSpan.FromSeconds(30), stop);
        }
    }
    
    private async Task EvaluateAsync()
    {
        // ───────────────────────────────────────────────────────────────
        // 0) Check: gibt es die benötigten Tabellen schon?
        //    (Beim Dev-Startup werden sie kurzzeitig gedroppt.)
        // ───────────────────────────────────────────────────────────────
        var tblCheck = await _db.ExecuteScalarAsync<int>(
            "SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name IN ('Events','EventMembers','Processes','ProcessSteps');");

        if (tblCheck < 4)           // Mindestens eine Tabelle fehlt ➜ geduldig zurück
            return;

        // ───────────────────────────────────────────────────────────────
        // 1) Status-Update „Offen  →  Geschlossen“, wenn voll
        // ───────────────────────────────────────────────────────────────
        var open   = (int)EventStatus.Offen;
        var closed = (int)EventStatus.Geschlossen;
        var trg    = (int)ProcessStepTrigger.MaxParticipantsReached;
        var act    = (int)ProcessStepAction.CloseEvent;

        const string sql = @"
UPDATE Events
SET    Status = @closed
WHERE  Status = @open
  AND  Id IN (
        SELECT p.EventId
        FROM   Processes     p
        JOIN   ProcessSteps  ps ON ps.ProcessId = p.Id
        WHERE  ps.Trigger = @trg
          AND  ps.Action  = @act
      )
  AND  MaxParticipants IS NOT NULL
  AND  (
        SELECT COUNT(1)
        FROM   EventMembers em
        WHERE  em.EventId       = Events.Id
          AND  em.IsParticipant = 1
      ) >= MaxParticipants;";

        int affected;
        try
        {
            affected = await _db.ExecuteAsync(sql, new { open, closed, trg, act });
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // Tabelle fehlt
        {
            // einmaliges Auftreten beim allerersten Start tolerieren
            return;
        }

        if (affected > 0)
            _log.LogInformation("ProcessWorker: {Cnt} Event(s) automatisch geschlossen", affected);
    }

}
