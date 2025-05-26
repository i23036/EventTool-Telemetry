using ET_Backend.Models;
using ET.Shared.DTOs;
using FluentResults;
using System.Security.Claims;

namespace ET_Backend.Services.Person
{
    /// <summary>
    /// Definiert die Schnittstelle für Benutzerservices.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gibt den aktuell eingeloggten Benutzer zurück.
        /// </summary>
        /// <param name="user">Der ClaimsPrincipal des eingeloggten Nutzers.</param>
        Task<Account?> GetCurrentUserAsync(ClaimsPrincipal user); // für EventController

        Task<Result<UserDto>> GetUserAsync(int id);                    
        Task<Result<List<MembershipDto>>> GetMembershipsAsync(int id); 
        Task<Result> UpdateEmailAsync(int accountId, string newEmail); 
        Task<Result> DeleteMembershipAsync(int accountId, int orgId);  

        /// <summary>
        /// Aktualisiert die Benutzerdaten.
        /// </summary>
        /// <param name="dto">Neue Benutzerdaten als DTO.</param>
        Task<Result> UpdateUserAsync(UserDto dto);
        Task<Result> DeleteUserAsync(int userId);
    }
}