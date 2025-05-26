using Microsoft.AspNetCore.Mvc;
using ET_Backend.Services.Processes;
using ET.Shared.DTOs;
using ET_Backend.Models;
using ET_Backend.Services.Mapping;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers;

/// <summary>
/// Stellt API-Endpunkte zur Verwaltung von Prozessen bereit.
/// </summary>
[ApiController]
[Route("api/process")]
public class ProcessController : ControllerBase
{
    private readonly IProcessService _processService;

    public ProcessController(IProcessService processService)
    {
        _processService = processService;
    }

    /// <summary>
    /// Holt einen Prozess anhand der ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCurrentProcess([FromBody] ProcessDto dto)
    {
        Process processModel = ProcessMapper.ToModel(dto);

        var result = await _processService.GetProcess(processModel);

        if (result.IsSuccess)
            return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Aktualisiert einen Prozess anhand der ID.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProcess([FromBody] ProcessDto dto)
    {
        Process processModel = ProcessMapper.ToModel(dto);

        var result = await _processService.UpdateProcess(processModel);

        if (result.IsSuccess)
            return Ok();

        return BadRequest(result.Errors);
    }
}