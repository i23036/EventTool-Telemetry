using ET_Backend.Models;

namespace ET_Backend.Repository.Person;

public interface IUserRepository
{
    public Task<bool> UserExists(int id);

    public Task<bool> CreateUser(String firstname, String lastname, String password);

    public Task<User> GetUser(int id);

    public Task<String> GetPasswordHash(int id);
}