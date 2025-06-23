using System.Data;
using Dapper;
using ET.Shared.DTOs.Enums;
using ET_Backend.Services.Helper;          // IEMailService

namespace ET_Backend.Services.Processes;

/// <summary>
/// Führt alle offenen Prozessschritte aus, sobald ihr Trigger (inkl. Offset) erfüllt ist.
/// Arbeitet mit einem eigenen DI-Scope je Zyklus, damit scoped Services (DB, Mail)
/// korrekt verwendet werden können.
/// </summary>
public sealed class ProcessWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory        _scopeFactory;
    private readonly ILogger<ProcessWorkerService> _log;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);   // ggf. AppSettings

    public ProcessWorkerService(
        IServiceScopeFactory          scopeFactory,
        ILogger<ProcessWorkerService> log)
    {
        _scopeFactory = scopeFactory;
        _log          = log;
    }

    // --------------------------------------------------------------------
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        _log.LogInformation("ProcessWorker läuft alle {s} Sekunden.", _interval.TotalSeconds);

        while (!stop.IsCancellationRequested)
        {
            try
            {
                // ► pro Zyklus ein neuer DI-Scope mit eigenen scoped Services
                using var scope = _scopeFactory.CreateScope();
                var db   = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                var mail = scope.ServiceProvider.GetRequiredService<IEMailService>();

                await EvaluateOnceAsync(db, mail);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "ProcessWorker: Fehler im Zyklus");
            }

            await Task.Delay(_interval, stop);
        }
    }

    // --------------------------------------------------------------------
    private async Task EvaluateOnceAsync(IDbConnection db, IEMailService mail)
    {
        const string sql = @"
SELECT  ps.Id, ps.Name, ps.Trigger, ps.Action, ps.Offset,
        ps.TriggeredByStepId, ps.ProcessId, ps.ExecutedAt, ps.Subject, ps.Body,

        e.Id             AS EventId,
        e.RegistrationStart,
        e.RegistrationEnd,
        e.MinParticipants,
        e.MaxParticipants,
        e.Status,

        (
            SELECT COUNT(1)
            FROM   EventMembers em
            WHERE  em.EventId = e.Id
              AND  em.IsParticipant = 1
        ) AS ParticipantCount,

        parent.ExecutedAt AS ParentExecutedAt
FROM    ProcessSteps ps
JOIN    Processes     p ON p.Id = ps.ProcessId
JOIN    Events        e ON e.Id = p.EventId
LEFT JOIN ProcessSteps parent ON parent.Id = ps.TriggeredByStepId
WHERE   ps.ExecutedAt IS NULL;";

        var steps = (await db.QueryAsync<StepRow>(sql)).ToList();
        if (steps.Count == 0) return;

        DateTime now = DateTime.UtcNow;

        foreach (var s in steps)
        {
            if (!IsTriggered(s, now)) continue;

            await RunActionAsync(db, mail, s);
            await db.ExecuteAsync(
                "UPDATE ProcessSteps SET ExecutedAt = @now WHERE Id = @id;",
                new { now, id = s.Id });

            _log.LogInformation("Step {Id} »{Name}« ausgeführt.", s.Id, s.Name);
        }
    }

    // --------------------------------------------------------------------
    private static bool IsTriggered(StepRow s, DateTime nowUtc) => (ProcessStepTrigger)s.Trigger switch
    {
        ProcessStepTrigger.MinParticipantsReached => s.ParticipantCount >= (s.MinParticipants + s.Offset ?? 0),
        ProcessStepTrigger.MaxParticipantsReached => s.ParticipantCount >= (s.MaxParticipants + s.Offset ?? 0),
        ProcessStepTrigger.OpenSubscription       => nowUtc >= s.RegistrationStart.AddDays(s.Offset ?? 0),
        ProcessStepTrigger.CloseSubscription      => nowUtc >= s.RegistrationEnd.AddDays(s.Offset ?? 0),
        ProcessStepTrigger.StepCompleted          => s.ParentExecutedAt is not null &&
                                                     nowUtc >= s.ParentExecutedAt.Value.AddDays(s.Offset ?? 0),
        _ => false
    };

    // ───────────────── Action-Dispatcher ─────────────────
    private async Task RunActionAsync(IDbConnection db, IEMailService mail, StepRow s)
    {
        _log.LogInformation("→ Prüfe Action {A} (Step {Id})", (ProcessStepAction)s.Action, s.Id);

        switch ((ProcessStepAction)s.Action)
        {
            case ProcessStepAction.OpenEvent:
                await UpdateStatusAsync(db, s.EventId, EventStatus.Offen);
                break;

            case ProcessStepAction.CloseEvent:
                await UpdateStatusAsync(db, s.EventId, EventStatus.Geschlossen);
                break;

            case ProcessStepAction.CancelEvent:
                await UpdateStatusAsync(db, s.EventId, EventStatus.Abgesagt);
                break;

            case ProcessStepAction.SendEmail:
                await SendMailAsync(db, mail, s);
                break;

            default:
                _log.LogWarning("Unbekannte Action {A} (Step {Id})", s.Action, s.Id);
                break;
        }
    }

    private static Task UpdateStatusAsync(IDbConnection db, int evtId, EventStatus status) =>
        db.ExecuteAsync("UPDATE Events SET Status = @st WHERE Id = @evt;",
                        new { st = (int)status, evt = evtId });

    // ───────────────── Mail-Versand ──────────────────────
    private async Task SendMailAsync(IDbConnection db, IEMailService mail, StepRow s)
    {
        var recipients = (await db.QueryAsync<string>(@"
        SELECT a.Email
        FROM   Accounts a
        JOIN   EventMembers em ON em.AccountId = a.Id
        WHERE  em.EventId = @evt
          AND  em.IsParticipant = 1;",
            new { evt = s.EventId })).Distinct().ToList();

        _log.LogInformation("Step {StepId}: Email-Action – {Cnt} Empfänger gefunden.",
            s.Id, recipients.Count);

        if (recipients.Count == 0)
            return;

        var subject = s.Subject ?? "Automatische Info zu deinem Event";
        var body = string.IsNullOrWhiteSpace(s.Body)
            ? "<p>Hallo! Dies ist eine automatisierte Nachricht zum Event.</p>"
            : s.Body;

        int successCount = 0;

        foreach (var addr in recipients)
        {
            try
            {
                await mail.SendAsync(addr, subject, body);
                await Task.Delay(100); // kurze Pause (anpassbar)
                successCount++;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Fehler beim Senden an {Email}", addr);
            }
        }

        _log.LogInformation("Step {StepId}: {Sent} von {Total} E-Mails erfolgreich verschickt.",
            s.Id, successCount, recipients.Count);
    }

    // --------------------------------------------------------------------
    private sealed class StepRow
    {
        public int      Id                { get; init; }
        public string   Name              { get; init; } = null!;
        public int      Trigger           { get; init; }
        public int      Action            { get; init; }
        public int?     Offset            { get; init; }
        public int?     TriggeredByStepId { get; init; }
        public DateTime? ExecutedAt       { get; init; }
        public string? Subject { get; init; }
        public string? Body    { get; init; }

        public int      EventId           { get; init; }
        public DateTime RegistrationStart { get; init; }
        public DateTime RegistrationEnd   { get; init; }
        public int      ParticipantCount  { get; init; }
        public int      MinParticipants   { get; init; }
        public int      MaxParticipants   { get; init; }

        public DateTime? ParentExecutedAt { get; init; }
    }
}
