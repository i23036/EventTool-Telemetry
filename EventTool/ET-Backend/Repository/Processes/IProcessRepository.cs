using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Processes;

public interface IProcessRepository
{
    Task<Result<Process>> GetProcess(int id);
}