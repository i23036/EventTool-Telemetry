using ET_Backend.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ET_Backend.Services.Person
{
    public interface IUserService
    {
        Task<Account?> GetCurrentUserAsync(ClaimsPrincipal user);
    }
}