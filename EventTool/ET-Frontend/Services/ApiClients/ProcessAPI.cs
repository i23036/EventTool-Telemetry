using ET_Frontend.Mapping;
using ET_Frontend.Models;
using ET.Shared.DTOs;
using System.Net.Http.Json;

namespace ET_Frontend.Services.ApiClients;

public class ProcessAPI : IProcessAPI
{
    private readonly HttpClient _http;
    public ProcessAPI(HttpClient http) => _http = http;

    public async Task<ProcessViewModel> GetAsync(int eventId)
        => ProcessViewMapper.ToViewModel(
            await _http.GetFromJsonAsync<ProcessDto>($"event/{eventId}/process")
            ?? new ProcessDto(0, new()));

    public async Task<bool> UpdateAsync(int eventId, ProcessViewModel vm)
        => (await _http.PutAsJsonAsync($"event/{eventId}/process",
            ProcessViewMapper.ToDto(vm))).IsSuccessStatusCode;
}