using Dapper;
using ET_Backend.Models;
using FluentResults;
using System.Data;

namespace ET_Backend.Repository.Processes;

/// <summary>
/// Dapper-basierte Implementierung für den Datenzugriff auf Prozessschritte.
/// </summary>
public class ProcessStepRepository : IProcessStepRepository
{
    private readonly IDbConnection _db;

    public ProcessStepRepository(IDbConnection db)
    {
        _db = db;
    }

    // === Lesen ===

    public async Task<Result<ProcessStep>> GetProcessStep(int id)
    {
        try
        {
            var step = await _db.QuerySingleOrDefaultAsync<ProcessStep>(
                @"SELECT Id, TypeName, Type, Trigger, Condition, OffsetInHours, ProcessId 
                  FROM ProcessSteps WHERE Id = @Id",
                new { Id = id });

            return step != null ? Result.Ok(step) : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessStep>>> GetAllProcessSteps(int processId)
    {
        try
        {
            var steps = (await _db.QueryAsync<ProcessStep>(
                @"SELECT Id, TypeName, Type, Trigger, Condition, OffsetInHours, ProcessId
              FROM ProcessSteps
              WHERE ProcessId = @ProcessId",
                new { ProcessId = processId })).ToList();

            return Result.Ok(steps);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }


    // === Schreiben ===

    public async Task<Result<ProcessStep>> CreateProcessStep(int processId, ProcessStep step)
    {
        try
        {
            var stepInsert = @"
                INSERT INTO ProcessSteps (TypeName, Type, Trigger, Condition, OffsetInHours, ProcessId)
                VALUES (@TypeName, @Type, @Trigger, @Condition, @OffsetInHours, @ProcessId);";

            var stepId = await _db.ExecuteScalarAsync<int>(
                stepInsert,
                new
                {
                    step.TypeName,
                    Type = (int)step.Type,
                    Trigger = (int)step.Trigger,
                    Condition = (int)step.Condition,
                    step.OffsetInHours,
                    ProcessId = processId
                });

            return await GetProcessStep(stepId);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    // === Löschen ===

    public async Task<Result<bool>> DeleteAllProcessSteps(int processId)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM ProcessSteps WHERE ProcessId = @ProcessId",
                new { ProcessId = processId });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }
}
