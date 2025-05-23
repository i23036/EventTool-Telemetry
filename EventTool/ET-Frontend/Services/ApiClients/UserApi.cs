using ET_Frontend.Models.AccountManagement;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ET_Frontend.Helpers;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Implementierung der Benutzer-API für das Frontend.
    /// </summary>
    public class UserApi : IUserApi
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Konstruktor mit HttpClient-Injektion.
        /// </summary>
        public UserApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<UserEditViewModel?> GetCurrentUserAsync()
        {
            var userId = JwtClaimHelper.GetUserIdAsync();
            return await _httpClient.GetFromJsonAsync<UserEditViewModel>($"user/{userId}");
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserAsync(UserEditViewModel model)
        {
            var userId = JwtClaimHelper.GetUserIdAsync();
            var response = await _httpClient.PutAsJsonAsync($"user/{userId}", model);
            return response.IsSuccessStatusCode;
        }
    }
}