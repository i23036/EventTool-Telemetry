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

    public async Task<ProcessDto?> GetForEvent(int eventId)
    {
        var res = await _repo.GetByEvent(eventId);     

        return res.IsSuccess ? ProcessMapper.ToDto(res.Value) : null;  
    }

    public async Task<Result> UpdateForEvent(int eventId, ProcessDto dto)
    {
        var model = ProcessMapper.ToModel(dto);
        model.EventId = eventId;

        return await _repo.Upsert(model);
    }
}