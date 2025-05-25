using ET_Frontend.Models.AccountManagement;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Schnittstelle zur Kapselung der API-Aufrufe für Benutzeraktionen.
    /// </summary>
    public interface IUserApi
    {
        /// <summary>
        /// Ruft die aktuellen Benutzerdaten vom Backend ab.
        /// </summary>
        /// <returns>Das UserEditViewModel mit den Benutzerdaten.</returns>
        Task<UserEditViewModel?> GetCurrentUserAsync();

        /// <summary>
        /// Aktualisiert die Benutzerdaten im Backend.
        /// </summary>
        /// <param name="model">Das geänderte ViewModel mit Benutzerinformationen.</param>
        /// <returns>True bei Erfolg, sonst false.</returns>
        Task<bool> UpdateUserAsync(UserEditViewModel model);
        Task<List<MembershipViewModel>> GetMembershipsAsync();
        Task<bool> UpdateEmailAsync(int accountId, string newEmail);
        Task<bool> DeleteMembershipAsync(int accountId, int orgId);
        Task<string?> SwitchAccountAsync(int accountId);
        Task<bool> AddMembershipAsync(string email);
    }
}