using ET_Frontend.Helpers;
using ET_Frontend.Models;
using ET_Frontend.Mapping;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Implementierung der Prozess-API für das Frontend.
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
        public async Task<bool> UpdateProcessAsync(ProcessViewModel model)
        {
            var userId = JwtClaimHelper.GetUserIdAsync(_authProvider);
            var dto = ProcessViewMapper.ToDto(model);
            var response = await _httpClient.PutAsJsonAsync($"process/{userId}", dto);
            return response.IsSuccessStatusCode;
        }
    }
}
