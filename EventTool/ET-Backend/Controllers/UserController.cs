using Microsoft.AspNetCore.Mvc;
using ET.Shared.DTOs;
using ET_Backend.Services.Person;
using FluentResults;

namespace ET_Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Aktualisiert einen Benutzer anhand der ID.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
    {
        var result = await _userService.UpdateUserAsync(id, dto);

        if (result.IsSuccess)
            return Ok();

        return BadRequest(result.Errors);
    }
}