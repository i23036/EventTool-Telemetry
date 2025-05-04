using ET_Backend.DTOs;
using ET_Backend.Services.Helper.Authentication;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ET_Backend.Controllers
{
    [Route("api/authenticate")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private IAuthenticateService _authenticateService;
        public AuthenticateController(IAuthenticateService authenticateService)
        {
            _authenticateService = authenticateService;
        }


        // POST api/<AuthenticateController>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto value)
        {
            Result<String> result = await _authenticateService.LoginUser(value.eMail, value.password);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return BadRequest(result.Value);
            }
        }

        // TODO: Registrierung

    }
}
