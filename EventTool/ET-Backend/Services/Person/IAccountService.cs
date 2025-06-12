using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Services.Person;

public interface IAccountService
{
    public Task<Result<Account>> GetAccountByMail(string accountEMail);
    public Task<Result<AccountService.ResolveResult>> ResolveEmailsAsync(
        List<string> organizerMails,
        List<string> contactMails,
        List<string> participantMails);
}