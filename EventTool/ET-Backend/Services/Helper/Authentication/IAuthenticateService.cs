using FluentResults;

namespace ET_Backend.Services.Helper.Authentication;

public interface IAuthenticateService
{
    public Task<Result<string>> LoginUser(string eMail, string password);
}