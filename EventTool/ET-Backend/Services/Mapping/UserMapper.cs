using ET_Backend.Models;
using ET.Shared.DTOs;

namespace ET_Backend.Services.Mapping;

/// <summary>
/// Stellt Mapping-Methoden zwischen User-Modell und User-DTO bereit.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Konvertiert ein UserDto in ein internes User-Modell.
    /// </summary>
    /// <param name="dto">Das DTO mit den übertragenen Benutzerdaten.</param>
    public static User ToModel(UserDto dto)
    {
        return new User
        {
            Id = dto.Id,
            Firstname = dto.FirstName,
            Lastname = dto.LastName,
            Password = dto.Password
        };
    }

    /// <summary>
    /// Konvertiert ein User-Modell in ein UserDto.
    /// </summary>
    /// <param name="user">Das interne Benutzerobjekt.</param>
    public static UserDto ToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Firstname,
            user.Lastname,
            user.Password
        );
    }
}