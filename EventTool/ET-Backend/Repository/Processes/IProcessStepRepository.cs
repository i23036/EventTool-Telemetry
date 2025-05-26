using ET_Backend.Models;
using FluentResults;
using System.Diagnostics;

namespace ET_Backend.Repository.Processes;

/// <summary>
/// Definiert Zugriffsmethoden auf Prozessschrittdaten in der Datenbank.
/// </summary>

public interface IProcessStepRepository
{
    // === Lesen ===
    Task<Result<ProcessStep>> GetProcessStep(int Id);

    Task<Result<List<ProcessStep>>> GetAllProcessSteps(int Id);

    // === Schreiben ===
    Task<Result<ProcessStep>> CreateProcessStep(int Id, ProcessStep processStepModel);

    // === Löschen ===
    Task<Result<bool>> DeleteAllProcessSteps(int Id);
}