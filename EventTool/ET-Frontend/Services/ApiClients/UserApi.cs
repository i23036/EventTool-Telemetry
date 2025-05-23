using ET_Frontend.Models.AccountManagement;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ET_Frontend.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Implementierung der Benutzer-API für das Frontend.
    /// </summary>
    public class UserApi : IUserApi
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authProvider;

        /// <summary>
        /// Konstruktor mit HttpClient- und AuthenticationStateProvider-Injektion.
        /// </summary>
        public UserApi(HttpClient httpClient, AuthenticationStateProvider authProvider)
        {
            _httpClient = httpClient;
            _authProvider = authProvider;
        }

        /// <inheritdoc />
        public async Task<UserEditViewModel?> GetCurrentUserAsync()
        {
            var userId = JwtClaimHelper.GetUserIdAsync(_authProvider);
            return await _httpClient.GetFromJsonAsync<UserEditViewModel>($"user/{userId}");
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserAsync(UserEditViewModel model)
        {
            var userId = JwtClaimHelper.GetUserIdAsync(_authProvider);
            var response = await _httpClient.PutAsJsonAsync($"user/{userId}", model);
            return response.IsSuccessStatusCode;
        }
    }
}