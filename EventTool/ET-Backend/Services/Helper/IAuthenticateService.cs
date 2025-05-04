using FluentResults;

namespace ET_Backend.Services.Helper;

public interface IAuthenticateService
{
    public Task<Result<String>> LoginUser(String eMail, String password);
}