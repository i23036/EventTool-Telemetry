using ET_Backend.Models;

namespace ET_Backend.Repository.Person;

public class AccountRepository : IAccountRepository
{
    public Task<bool> AccountExists(String eMail)
    {
        throw new NotImplementedException();
    }

    public Task<Account> GetAccount(String eMail)
    {
        throw new NotImplementedException();
    }

    public Task<String> GetPasswordHash(String eMail)
    {
        throw new NotImplementedException();
    }
}