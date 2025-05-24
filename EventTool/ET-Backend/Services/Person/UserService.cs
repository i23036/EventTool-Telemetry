using System.Security.Claims;
using ET_Backend.Models;
using ET_Backend.Repository.Person;
using ET_Backend.Services.Mapping;
using ET.Shared.DTOs;
using FluentResults;

namespace ET_Backend.Services.Person
{
    /// <summary>
    /// Implementiert die Geschäftslogik für Benutzeroperationen.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <inheritdoc />
        public async Task<Account?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            return null; // Wird aktuell nicht über UserRepository abgebildet
        }

        /// <inheritdoc />
        public async Task<Result> UpdateUserAsync(int id, UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                return Result.Fail("Name und Nachname dürfen nicht leer sein.");

            var getResult = await _userRepository.GetUser(id);
            if (getResult.IsFailed || getResult.Value is null)
                return Result.Fail("Benutzer nicht gefunden.");

            var user = getResult.Value;

            user.Firstname = dto.FirstName;
            user.Lastname = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;

            return await _userRepository.EditUser(user);
        }
    }
}