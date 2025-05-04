using ET_Backend.Models;
using ET_Backend.Repository.Person;
using FluentResults;

namespace ET_Backend.Services.Helper;

public class AuthenticateService : IAuthenticateService
{
    private IAccountRepository _repository;

    public AuthenticateService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<String>> LoginUser(String eMail, String password)
    {
        if (await _repository.AccountExists(eMail))
        {
            String passwordHash = await _repository.GetPasswordHash(eMail);
            if (passwordHash == password)
            {
                String token = GenerateJwtToken(await _repository.GetAccount(eMail));
                return Result.Ok<String>(token);
            }
            else
            {
                return Result.Fail<String>("Password does not match");
            }
        }
        else
        {
            return Result.Fail<String>("User does not exist");
        }
    }

    private String GenerateJwtToken(Account account)
    {
        var token = new JwtSe
    }
}