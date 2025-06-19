using ET.Shared.DTOs;
using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Services.Processes;

public interface IProcessService
{
    Task<ProcessDto> GetForEvent(int eventId);
    Task<Result>     UpdateForEvent(int eventId, ProcessDto dto);
}