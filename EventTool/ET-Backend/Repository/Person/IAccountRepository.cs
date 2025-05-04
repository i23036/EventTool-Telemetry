using ET_Backend.Models;

namespace ET_Backend.Repository.Person;

public interface IAccountRepository
{
    public Task<bool> AccountExists(String eMail);

    public Task<Account> CreateAccount(String eMail, Models.Organization organization, Role role);

    public Task<Account> GetAccount(String eMail);

    public Task<String> GetPasswordHash(String eMail);
}