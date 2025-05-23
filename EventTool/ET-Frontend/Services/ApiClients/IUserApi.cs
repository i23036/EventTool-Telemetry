using ET_Frontend.Models.AccountManagement;
using System.Threading.Tasks;

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
    }
}