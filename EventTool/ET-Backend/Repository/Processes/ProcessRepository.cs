using Dapper;
using ET_Backend.Models;
using FluentResults;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;

namespace ET_Backend.Repository.Processes;

/// <summary>
/// Dapper-basierte Implementierung für den Datenzugriff auf Organisationen.
/// </summary>

public class ProcessRepository : IProcessRepository
{
    private readonly IDbConnection _db;

    public ProcessRepository(IDbConnection db)
    {
        _db = db;
    }

    // === Lesen ===

    public async Task<Result<Models.Process>> GetProcess(int id)
    {
        try
        {
            var proc = await _db.QuerySingleOrDefaultAsync<Models.Process>(
                "SELECT Id FROM Processes WHERE Id = @Id",
                new { Id = id});

            return proc != null ? Result.Ok(proc) : Result.Fail("NotFound");
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    // === Schreiben ===

    public async Task<Result<Models.Process>> CreateProcess()
    {
        try
        {
            var procInsert = @"
            INSERT INTO Processes DEFAULT VALUES;";

            var procId = await _db.ExecuteScalarAsync<int>(
                procInsert);

            return await GetProcess(procId);
        }
        catch (Exception ex)
        {
            return Result.Fail($"DBError: {ex.Message}");
        }
    }

    public async Task<Result<Models.Process>> UpdateProcess(int id)
    {
        return await GetProcess(id); //hier nichts los weil nur ID, aber wenn weitere Werte dazukommen würden -> Sinnvoll
    }

    public async Task<Result<bool>> DeleteProcess(int id)
    {
        try
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM Processes WHERE Id = @Id",
                new { Id = id });

            return rows > 0 ? Result.Ok() : Result.Fail("NotFound");
        }
        catch (Exception)
        {
            return Result.Fail("DBError");
        }
    }
}