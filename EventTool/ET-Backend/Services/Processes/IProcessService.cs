using FluentResults;
using ET_Backend.Models;

namespace ET_Backend.Services.Processes;

public interface IProcessService
{

    // === Existenzprüfung ===

    /// <summary>Prüft ob der angegebene Prozess existiert.</summary>
    Task<Result<bool>> ProcessExists(Process processModel);

    /// <summary>Prüft ob der angegebene Prozessschritt existiert.</summary>
    Task<Result<bool>> ProcessStepExists(ProcessStep processStepModel);

    // === Lesen ===

    /// <summary>Gibt einen Prozess anhand eines Models.</summary>
    Task<Result<Models.Process>> GetProcess(Process processModel);

    /// <summary>Gibt einen Prozess anhand des dazugehörigen Events (und dessen ID) zurück.</summary>
    Task<Result<Models.Process>> GetProcess(); //TODO ob ja nein gebraucht

    /// <summary>Gibt einen Prozessschritt anhand eines Models.</summary>
    Task<Result<Models.ProcessStep>> GetProcessStep(ProcessStep processStepModel);

    /// <summary>Gibt alle Prozessschritte anhand eines Models.</summary>
    Task<Result<List<Models.ProcessStep>>> GetAllProcessSteps(Process processModel);

    // === Erstellen & Bearbeiten ===

    /// <summary>Erstellt einen neuen Prozess.</summary>
    Task<Result<Process>> CreateProzess();

    /// <summary>Erstellt einen neuen Prozessschritt zum passenden Prozess und den übergeben Daten (aus dem Frontend).</summary>
    Task<Result<ProcessStep>> CreateProcessStep(Process processModel, ProcessStep processStepModel);

    /// <summary>Aktualisiert einen Prozess mit Model-Daten (z. B. aus dem Frontend).</summary>
    Task<Result<bool>> UpdateProcess(Process processModel);

    /// <summary>Aktualisiert einen Prozessschritt mit Model-Daten (z. B. aus dem Frontend).</summary>
    Task<Result<bool>> UpdateProcessStep(ProcessStep processStepModel);

    // === Löschen ===

    /// <summary>Löscht einen Prozess mit allen dazugehörigen Prozesschritten anhand seiner ID.</summary>
    Task<Result<bool>> DeleteProcess(Process processModel);

    /// <summary>Löscht einen Prozessschritt anhand der ID.</summary>
    Task<Result<bool>> DeleteAllProcessSteps(Process processModel);
}