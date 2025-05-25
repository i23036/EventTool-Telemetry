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
        public async Task<Result> UpdateUserAsync(UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                return Result.Fail("Name und Nachname dürfen nicht leer sein.");

            var getResult = await _userRepository.GetUser(dto.Id);
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

        public async Task<Result<UserDto>> GetUserAsync(int id)
        {
            var get = await _userRepository.GetUser(id);
            if (get.IsFailed || get.Value is null)
                return Result.Fail("Benutzer nicht gefunden.");

            return Result.Ok(UserMapper.ToDto(get.Value));   // Password wird dort schon geleert
        }

        public async Task<Result<List<MembershipDto>>> GetMembershipsAsync(int id)
        {
            var accs = await _accountRepo.GetAccountsByUser(id);
            if (accs.IsFailed) return Result.Fail(accs.Errors);

            var list = accs.Value.Select(a => new MembershipDto(
                a.Id,
                a.Organization.Id,
                a.Organization.Name,
                a.EMail)).ToList();

            return Result.Ok(list);
        }

        public async Task<Result> UpdateEmailAsync(int accountId, string newEmail)
        {
            // einfache E-Mail-Validierung
            if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains('@'))
                return Result.Fail("Ungültige E-Mail-Adresse.");

            return await _accountRepo.UpdateEmail(accountId, newEmail);
        }

        public async Task<Result> DeleteMembershipAsync(int accountId, int orgId)
            => await _accountRepo.RemoveFromOrganization(accountId, orgId);
    }
}
