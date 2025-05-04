using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ET_Backend.Models;
using ET_Backend.Repository.Person;
using FluentResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ET_Backend.Services.Helper.Authentication;

public class AuthenticateService : IAuthenticateService
{
    private IAccountRepository _repository;
    private JwtOptions _jwtOptions;

    public AuthenticateService(IAccountRepository repository, IOptions<JwtOptions> jwtOptions)
    {
        _repository = repository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<string>> LoginUser(string eMail, string password)
    {
        if (await _repository.AccountExists(eMail))
        {
            string passwordHash = await _repository.GetPasswordHash(eMail);
            if (passwordHash == password)
            {
                string token = GenerateJwtToken(await _repository.GetAccount(eMail));
                return Result.Ok(token);
            }
            else
            {
                return Result.Fail<string>("Password does not match");
            }
        }
        else
        {
            return Result.Fail<string>("User does not exist");
        }
    }

    private string GenerateJwtToken(Account account)
    {
        Claim[] claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Email, account.EMail),
            
            // TODO: Mehr hinzufügen
        };

        SigningCredentials signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256
            );

        JwtSecurityToken token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audiece,
            claims,
            null,
            DateTime.UtcNow.AddHours(_jwtOptions.ExperationTime),
            signingCredentials
            );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}