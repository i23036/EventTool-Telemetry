using ET_Frontend.Models.AccountManagement;
using ET_Frontend.Helpers;
using ET_Frontend.Mapping;
using ET.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Implementierung der Benutzer-API für das Frontend.
    /// </summary>
    public class UserApi : IUserApi
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authProvider;

        public UserApi(HttpClient httpClient, AuthenticationStateProvider authProvider)
        {
            _httpClient = httpClient;
            _authProvider = authProvider;
        }

        public async Task<UserEditViewModel?> GetCurrentUserAsync()
        {
            var userId = await JwtClaimHelper.GetUserIdAsync(_authProvider);
            var dto = await _httpClient.GetFromJsonAsync<UserDto>($"api/user/{userId}");

            return dto == null ? null : UserViewMapper.ToViewModel(dto);
        }

        public async Task<bool> UpdateUserAsync(UserEditViewModel model)
        {
            var userId = await JwtClaimHelper.GetUserIdAsync(_authProvider);
            var dto = UserViewMapper.ToDto(model);

            var response = await _httpClient.PutAsJsonAsync($"api/user/{userId}", dto);
            return response.IsSuccessStatusCode;
        }
    }
}