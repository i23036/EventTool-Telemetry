using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Person;
/// <summary>
/// Definiert Methoden zum Zugriff auf Benutzerdaten in der Datenquelle.
/// </summary>
public interface IUserRepository
{
    public Task<Result<bool>> UserExists(int userId);
    public Task<Result<User>> CreateUser(String firstname, String lastname, String password);
    public Task<Result> DeleteUser(int userId);
    public Task<Result<User>> GetUser(int userId);
    public Task<Result> EditUser(User user);
}