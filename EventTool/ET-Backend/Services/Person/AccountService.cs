using ET_Backend.Models;
using ET_Backend.Repository.Person;
using FluentResults;

namespace ET_Backend.Services.Person;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<Account>> GetAccountByMail(string accountEMail)
    {
        return await _accountRepository.GetAccount(accountEMail);
    }

    public async Task<Result<ResolveResult>> ResolveEmailsAsync(
        List<string> organizerMails,
        List<string> contactMails,
        List<string> participantMails)
    {
        var allMails = organizerMails
            .Concat(contactMails)
            .Concat(participantMails)
            .Distinct()
            .ToList();

        var accRes = await _accountRepository.GetAccountsByMail(allMails);
        if (accRes.IsFailed) return Result.Fail(accRes.Errors);

        var dict = accRes.Value.ToDictionary(a => a.EMail, a => a);

        List<Account> Pick(IEnumerable<string> emails) =>
            emails.Where(dict.ContainsKey).Select(m => dict[m]).ToList();

        return Result.Ok(new ResolveResult(
            Pick(organizerMails),
            Pick(contactMails),
            Pick(participantMails)));
    }
    public record ResolveResult(
        List<Account> Organizers,
        List<Account> ContactPersons,
        List<Account> Participants);
}