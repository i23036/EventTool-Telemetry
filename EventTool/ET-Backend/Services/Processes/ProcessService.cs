using ET.Shared.DTOs;
using FluentResults;
using ET_Backend.Models;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using ET_Backend.Repository.Processes;
using System.Diagnostics;

namespace ET_Backend.Services.Processes;

/// <summary>
/// Implementierung des IProcessService für Prozesse.
/// </summary>

public class ProcessService : IProcessService
{
    private readonly IProcessRepository _processRepository;
    private readonly IProcessStepRepository _processStepRepository;
    public ProcessService(IProcessRepository processRepository, IProcessStepRepository processStepRepository)
    {
        _processRepository = processRepository;
        _processStepRepository = processStepRepository;
    }

    // === Existenzprüfung ===

    /// <summary>Prüft ob der angegebene Prozess existiert.</summary>
    public async Task<Result<bool>> ProcessExists(Models.Process processModel)
    {
        // await _processRepository.ProcessExists(processModel.Id); deprecated

        return Result.Ok();
    }

    /// <summary>Prüft ob der angegebene Prozessschritt existiert.</summary>
    public async Task<Result<bool>> ProcessStepExists(Models.ProcessStep processStepModel)
    {
        // await _processStepRepository.ProcessStepExists(processStepModel.Id); deprecated

        return Result.Ok();
    }

    // === Lesen ===

    /// <summary>Gibt einen Prozess anhand seiner ID zurück.</summary>
    public async Task<Result<Models.Process>> GetProcess(Models.Process processModel)
    {
        var result1 = await _processRepository.GetProcess(processModel.Id);
        var result2 = await _processStepRepository.GetAllProcessSteps(processModel.Id);

        if (result1.IsSuccess && result2.IsSuccess)
        {
            return new Models.Process
            {
                Id = result1.Value.Id,
                ProcessSteps = result2.Value
            };
        }
        else
        {
            return Result.Fail("error in Service");
        }
    }

    /// <summary>Gibt einen Prozess anhand des dazugehörigen Events (und dessen ID) zurück.</summary>
    public async Task<Result<Models.Process>> GetProcess()
    {
        return Result.Ok(); //TODO vor now
    }

    /// <summary>Gibt einen Prozessschritt anhand seiner ID zurück.</summary>
    public async Task<Result<Models.ProcessStep>> GetProcessStep(Models.ProcessStep processStepModel)
    {
        return await _processStepRepository.GetProcessStep(processStepModel.Id);
    }

    /// <summary>Gibt alle Prozessschritte anhand der Proess ID zurück.</summary>
    public async Task<Result<List<Models.ProcessStep>>> GetAllProcessSteps(Models.Process processModel)
    {
        return await _processStepRepository.GetAllProcessSteps(processModel.Id);
    }

    // === Erstellen & Bearbeiten ===

    /// <summary>Erstellt einen neuen Prozess.</summary>
    public async Task<Result<Models.Process>> CreateProzess()
    {
        return await _processRepository.CreateProcess();
    }

    /// <summary>Erstellt einen neuen Prozessschritt zum passenden Prozess (über Id).</summary>
    public async Task<Result<Models.ProcessStep>> CreateProcessStep(Models.Process processModel, Models.ProcessStep processStepModel)
    {
        return await _processStepRepository.CreateProcessStep(processModel.Id, processStepModel);
    }

    /// <summary>Aktualisiert einen Prozess mit Model-Daten (z. B. aus dem Frontend).</summary>
    public async Task<Result<bool>> UpdateProcess(Models.Process processModel)
    {
        var result1 = await _processRepository.UpdateProcess(processModel.Id); //hier passiert nicht viel, aber gut für Erweiterung wenn es zum Beispiel rozessvorlagen wieder geben soll
        if (result1.IsFailed)
        {
            return Result.Fail("Error in Service");
        }

        var result2 = await DeleteAllProcessSteps(processModel);
        if (result2.IsFailed)
        {
            return Result.Fail("Error in Service");
        }

        foreach (Models.ProcessStep processStepModel in processModel.ProcessSteps)
        {
            var result3 = await CreateProcessStep(processModel, processStepModel);
            if (result3.IsFailed)
            {
                return Result.Fail("Error in Service");
            }
        }
        return Result.Ok(true);
        
    }

    /// <summary>Aktualisiert einen Prozessschritt mit Model-Daten (z. B. aus dem Frontend).</summary>
    public async Task<Result<bool>> UpdateProcessStep(Models.ProcessStep processStepModel)
    {
        //await _processStepRepository.UpdateProcessStep(processStepModel); depricated
        return Result.Ok();
    }



    // === Löschen ===

    /// <summary>Löscht einen Prozess anhand der ID.</summary>
    public async Task<Result<bool>> DeleteProcess(Models.Process processModel)
    {
        await DeleteAllProcessSteps(processModel); //löscht zuvor alle verknüpften Prozesschritte

        var result = await _processRepository.DeleteProcess(processModel.Id);
        if (result.IsSuccess)
        {
            return Result.Ok(true);
        }
        else
        {
            return Result.Fail("Error in Service");
        }
    }

    /// <summary>Löscht alle Prozessschritte eines Prozesses anhand seiner ID.</summary>
    public async Task<Result<bool>> DeleteAllProcessSteps(Models.Process processModel)
    {
        var result = await _processStepRepository.DeleteAllProcessSteps(processModel.Id);
        if (result.IsSuccess)
        {
            return Result.Ok(true);
        }
        else
        {
            return Result.Fail("Error in Service");
        }
    }
}