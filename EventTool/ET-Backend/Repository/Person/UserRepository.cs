using ET_Backend.Models;
using FluentResults;

namespace ET_Backend.Repository.Person;

public class UserRepository : IUserRepository
{
    public Task<Result<bool>> UserExists(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Result> CreateUser(String firstname, String lastname, String password)
    {
        throw new NotImplementedException();
    }

    public Task<Result<User>> GetUser(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<String>> GetPasswordHash(int id)
    {
        throw new NotImplementedException();
    }
}