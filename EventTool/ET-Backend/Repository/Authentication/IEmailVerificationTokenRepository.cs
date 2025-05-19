using FluentResults;

namespace ET_Backend.Repository.Authentication;

public interface IEmailVerificationTokenRepository
{
    Task<Result> CreateAsync(int accountId, string token);
    Task<Result<(int AccountId, DateTime ExpiresAt)>> GetAsync(string token);
    Task<Result> ConsumeAsync(string token);
}