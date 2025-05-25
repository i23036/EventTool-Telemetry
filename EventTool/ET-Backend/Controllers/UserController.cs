using System.Security.Claims;
using ET_Backend.Services.Helper.Authentication;
using Microsoft.AspNetCore.Mvc;
using ET.Shared.DTOs;
using ET_Backend.Services.Person;
using FluentResults;
using Microsoft.AspNetCore.Authorization;

namespace ET_Backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthenticateService _authenticateService;

    public UserController(IUserService userService, IAuthenticateService authenticateService)
    {
        _userService = userService;
        _authenticateService = authenticateService;
    }

    /// <summary>
    /// Aktualisiert einen Benutzer anhand der ID.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto dto)
    {
        var result = await _userService.UpdateUserAsync(dto);

        if (result.IsSuccess)
            return Ok();

        return BadRequest(result.Errors);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var result = await _userService.GetUserAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpGet("{id:int}/memberships")]
    [Authorize]
    public async Task<ActionResult<List<MembershipDto>>> GetMemberships(int id)
    {
        var result = await _userService.GetMembershipsAsync(id);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPut("memberships/{accountId:int}/email")]
    [Authorize]
    public async Task<IActionResult> UpdateEmail(int accountId, [FromBody] string newEmail)
    {
        var res = await _userService.UpdateEmailAsync(accountId, newEmail);
        return res.IsSuccess ? Ok() : BadRequest(res.Errors);
    }

    [HttpDelete("memberships/{accountId:int}/{orgId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMembership(int accountId, int orgId)
    {
        var res = await _userService.DeleteMembershipAsync(accountId, orgId);
        return res.IsSuccess ? Ok() : BadRequest(res.Errors);
    }

    [HttpPost("memberships/add")]
    [Authorize]
    public async Task<IActionResult> AddMembership([FromBody] string email)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await _authenticateService.AddMembership(userId, email);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

}