using System.Security.Claims;
using ET_Backend.Models;
using ET_Backend.Repository.Person;

namespace ET_Backend.Services.Person
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository _accountRepo;

        public UserService(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        public async Task<Account?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return null;

            var result = await _accountRepo.GetAccount(email);
            if (result.IsSuccess)
                return result.Value;

            return null;
        }
    }
}