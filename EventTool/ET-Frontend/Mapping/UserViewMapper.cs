using ET.Shared.DTOs;
using ET_Frontend.Models.AccountManagement;

namespace ET_Frontend.Mapping;

/// <summary>
/// Enthält Mapping-Methoden zwischen UserDto und UserEditViewModel.
/// </summary>
public static class UserViewMapper
{
    /// <summary>
    /// Wandelt einen UserDto in ein ViewModel zur Anzeige/Bearbeitung um.
    /// </summary>
    public static UserEditViewModel ToViewModel(UserDto dto)
    {
        return new UserEditViewModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Password = string.Empty,
            Reppassword = string.Empty
        };
    }

    /// <summary>
    /// Wandelt ein ViewModel zurück in einen UserDto zur API-Übertragung.
    /// </summary>
    public static UserDto ToDto(UserEditViewModel vm)
    {
        return new UserDto(
            vm.FirstName,
            vm.LastName,
            vm.Password
        );
    }
}