using ET_Frontend.Helpers;
using ET_Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Implementierung der Benutzer-API für das Frontend.
    /// </summary>
    public class ProcessAPI : IProcessAPI
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authProvider;

        /// <summary>
        /// Konstruktor mit HttpClient- und AuthenticationStateProvider-Injektion.
        /// </summary>
        public ProcessAPI(HttpClient httpClient, AuthenticationStateProvider authProvider)
        {
            _httpClient = httpClient;
            _authProvider = authProvider;
        }

        /// <inheritdoc />
        public async Task<ProcessModel?> GetCurrentProcessAsync()
        {
            var userId = JwtClaimHelper.GetUserIdAsync(_authProvider);
            return await _httpClient.GetFromJsonAsync<ProcessModel>($"process/{userId}");
        }

        /// <inheritdoc />
        public async Task<bool> UpdateProcessAsync(ProcessModel model)
        {
            var userId = JwtClaimHelper.GetUserIdAsync(_authProvider);
            var response = await _httpClient.PutAsJsonAsync($"process/{userId}", model);
            return response.IsSuccessStatusCode;
        }
    }
}
}
