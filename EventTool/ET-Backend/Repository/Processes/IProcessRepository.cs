using FluentResults;
using System.Diagnostics;

namespace ET_Backend.Repository.Processes;

/// <summary>
/// Definiert Zugriffsmethoden auf Prozessdaten in der Datenbank.
/// </summary>

public interface IProcessRepository
{
    // === Lesen ===

    Task<Result<Models.Process>> GetProcess(int Id);

    // === Schreiben ===

    Task<Result<Models.Process>> CreateProcess();

    Task<Result<Models.Process>> UpdateProcess(int Id);

    // === Löschen ===
    Task<Result<bool>> DeleteProcess(int Id);
}