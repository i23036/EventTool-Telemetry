using ET_Backend.Models;
using ET_Backend.Models.Enums;
using FluentResults;

namespace ET_Backend.Repository.Person;

/// <summary>
/// Definiert Methoden für den Zugriff auf Kontodaten in der Datenquelle.
/// </summary>
public interface IAccountRepository
{
    public Task<Result<bool>> AccountExists(string accountEMail);
    public Task<Result<bool>> AccountExists(int accountId);
    public Task<Result<Account>> CreateAccount(string accountEMail, Models.Organization organization, Role role, User user);
    public Task<Result> DeleteAccount(string accountEMail);
    public Task<Result> DeleteAccount(int accountId);
    public Task<Result<Account>> GetAccount(string accountEMail);
    public Task<Result<Account>> GetAccount(int accountId);
    public Task<Result> EditAccount(Account account);
    public Task<Result> RemoveFromOrganization(int accountId, int orgId);
    public Task<Result<List<Account>>> GetAccountsByUser(int userId);
    public Task<Result<List<Account>>> GetAccountsByMail(IEnumerable<string> mails);
    public Task<Result> UpdateEmail(int accountId, string email);
    public Task<Result> UpdateEmailDomainsForOrganization(int orgId, string oldDomain, string newDomain);
}