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
        Task<Account?> GetCurrentUserAsync(ClaimsPrincipal user);

        /// <summary>
        /// Aktualisiert die Benutzerdaten.
        /// </summary>
        /// <param name="id">ID des zu bearbeitenden Benutzers.</param>
        /// <param name="dto">Neue Benutzerdaten als DTO.</param>
        Task<Result> UpdateUserAsync(int id, UserDto dto);
    }
}