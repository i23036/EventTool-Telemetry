using ET.Shared.DTOs;
using ET_Backend.Models;
using ET_Backend.Repository.Processes;
using ET_Backend.Services.Mapping;
using FluentResults;

namespace ET_Backend.Services.Processes;

/// <summary>
/// Zentrale Logik für die Event-gebundenen Prozesse.
/// Die Klasse ist bewusst schlank: alle Datenbank-Operationen laufen über <see cref="IProcessRepository"/>.
/// </summary>
public class ProcessService : IProcessService
{
    private readonly IProcessRepository _repo;

    public ProcessService(IProcessRepository repo)
    {
        _repo = repo;
    }

    // ========= Lesen =========
    public async Task<ProcessDto> GetForEvent(int eventId)
    {
        var result = await _repo.GetByEvent(eventId);     // <─ GetByEvent ist async
        if (result.IsFailed)
            throw new InvalidOperationException(
                $"Kein Prozess für EventId={eventId}: {string.Join(',', result.Errors)}");

        return ProcessMapper.ToDto(result.Value);         // ➜ DTO fürs Frontend
    }

// ========= Schreiben =========
    public async Task<Result> UpdateForEvent(int eventId, ProcessDto dto)
    {
        // aktuellen Prozess mit Id besorgen
        var current = await _repo.GetByEvent(eventId);
        if (current.IsFailed) return current.ToResult();

        var model = ProcessMapper.ToModel(dto);
        model.Id      = current.Value.Id;  // Id behalten
        model.EventId = eventId;           // Sicherheit

        return await _repo.Upsert(model);  // Steps ersetzen
    }
}