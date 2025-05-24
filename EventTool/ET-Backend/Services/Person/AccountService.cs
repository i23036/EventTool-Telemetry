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
}