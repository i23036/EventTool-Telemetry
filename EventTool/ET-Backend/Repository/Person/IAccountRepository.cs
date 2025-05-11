using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Person;

/// <summary>
/// Definiert Methoden für den Zugriff auf Kontodaten in der Datenquelle.
/// </summary>
public interface IAccountRepository
{
    public Task<Result<bool>> AccountExists(String accountEMail);
    public Task<Result<bool>> AccountExists(int accountId);
    public Task<Result<Account>> CreateAccount(String accountEMail, Models.Organization organization, Role role, User user);
    public Task<Result> DeleteAccount(String accountEMail);
    public Task<Result> DeleteAccount(int accountId);
    public Task<Result<Account>> GetAccount(String accountEMail);
    public Task<Result<Account>> GetAccount(int accountId);
    public Task<Result> EditAccount(Account account);

}