using ET.Shared.DTOs;
using ET_Frontend.Helpers;
using ET_Frontend.Mapping;
using ET_Frontend.Models.AccountManagement;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ET_Frontend.Services.ApiClients
{
    /// <summary>
    /// Implementierung der Benutzer-API für das Frontend.
    /// </summary>
    public class UserApi : IUserApi
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _auth;

        public UserApi(HttpClient http, AuthenticationStateProvider auth)
        {
            _http = http; _auth = auth;
        }

        public async Task<UserEditViewModel?> GetCurrentUserAsync()
        {
            var id = await JwtClaimHelper.GetUserIdAsync(_auth);
            var dto = await _http.GetFromJsonAsync<UserDto>($"api/user/{id}");
            return dto is null ? null : UserViewMapper.ToViewModel(dto);
        }

        public async Task<bool> UpdateUserAsync(UserEditViewModel vm)
        {
            var id = await JwtClaimHelper.GetUserIdAsync(_auth);
            var dto = UserViewMapper.ToDto(vm, id);
            var res = await _http.PutAsJsonAsync($"api/user/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<List<MembershipViewModel>> GetMembershipsAsync()
        {
            var id = await JwtClaimHelper.GetUserIdAsync(_auth);
            var list = await _http.GetFromJsonAsync<List<MembershipDto>>(
                $"api/user/{id}/memberships") ?? new List<MembershipDto>();

            return list.Select(DtoMembershipMapper.FromDto).ToList();
        }

        public async Task<bool> UpdateEmailAsync(int accountId, string newEmail)
        {
            var res = await _http.PutAsJsonAsync($"api/user/memberships/{accountId}/email", newEmail);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMembershipAsync(int accountId, int orgId)
        {
            var res = await _http.DeleteAsync($"api/user/memberships/{accountId}/{orgId}");
            return res.IsSuccessStatusCode;
        }

        public async Task<string?> SwitchAccountAsync(int accountId)
        {
            var res = await _http.PostAsync($"api/authenticate/switch/{accountId}", null);
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetString();
        }

        public async Task<bool> AddMembershipAsync(string email)
        {
            var response = await _http.PostAsJsonAsync("api/user/memberships/add", email);
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteCurrentUserAsync()
        {
            var response = await _http.DeleteAsync("api/user");
            response.EnsureSuccessStatusCode();
        }
    }
}