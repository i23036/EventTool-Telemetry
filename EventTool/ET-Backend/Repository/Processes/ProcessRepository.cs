using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Processes;

public class ProcessRepository : IProcessRepository
{
    public async Task<Result<Process>> GetProcess(int id)
    {
        return null;
    }
}