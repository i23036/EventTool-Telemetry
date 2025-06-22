using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Processes;

/// <summary>
/// Definiert Zugriffsmethoden auf Prozessdaten in der Datenbank.
/// </summary>

public interface IProcessRepository
{
    // === Lesen ===
    Task<Result<Process>> GetByEvent(int eventId);
    
    // === Schreiben ===
    Task<Result>          Upsert(Process proc);
}