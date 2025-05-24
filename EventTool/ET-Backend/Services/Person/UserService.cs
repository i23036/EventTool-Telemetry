using System.Security.Claims;
using ET_Backend.Models;
using ET_Backend.Repository.Person;
using ET_Backend.Services.Mapping;
using ET.Shared.DTOs;
using FluentResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ET_Backend.Services.Person
{
    /// <summary>
    /// Implementiert die Geschäftslogik für Benutzeroperationen.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepo;

        public UserService(IUserRepository userRepository, IAccountRepository accountRepo)
        {
            _userRepository = userRepository;
            _accountRepo = accountRepo;
        }

        /// <inheritdoc />
        public async Task<Account?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            // E-Mail-Claim aus dem JWT holen (Custom-Claim: 'email')
            var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return null;

            var result = await _accountRepo.GetAccount(email);
            return result.IsSuccess ? result.Value : null;
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

            // Nur aktualisieren, wenn Passwort gesetzt wurde
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;

            return await _userRepository.EditUser(user);
        }
    }
}
