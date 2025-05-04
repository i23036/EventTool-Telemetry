using ET_Backend.Models;

namespace ET_Backend.Repository.Person;

public class UserRepository : IUserRepository
{
    public Task<bool> UserExists(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateUser(String firstname, String lastname, String password)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUser(int id)
    {
        throw new NotImplementedException();
    }

    public Task<String> GetPasswordHash(int id)
    {
        throw new NotImplementedException();
    }
}