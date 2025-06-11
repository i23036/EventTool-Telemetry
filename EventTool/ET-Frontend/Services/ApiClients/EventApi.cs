using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.SessionStorage;
using ET.Shared.DTOs;

namespace ET_Frontend.Services.ApiClients;

/// <summary>
///  Kapselt alle Event-bezogenen HTTP-Aufrufe.
/// </summary>
public class EventApi : IEventApi
{
    private readonly HttpClient _http;
    private readonly ISessionStorageService _session;

    public EventApi(HttpClient http, ISessionStorageService session)
    {
        _http    = http;
        _session = session;
    }

    public async Task<bool> CreateEventAsync(EventDto dto)
    {
        var req = await BuildRequest(HttpMethod.Post, "api/event");
        req.Content = JsonContent.Create(dto);

        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<EventDto?> GetEventAsync(int eventId)
    {
        var req = await BuildRequest(HttpMethod.Get, $"api/event/{eventId}");
        var resp = await _http.SendAsync(req);

        if (!resp.IsSuccessStatusCode)
            return null;

        return await resp.Content.ReadFromJsonAsync<EventDto>();
    }

    public async Task<bool> SubscribeAsync(int eventId)
    {
        var req  = await BuildRequest(HttpMethod.Put, $"api/event/subscribe/{eventId}");
        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> UnsubscribeAsync(int eventId)
    {
        var req  = await BuildRequest(HttpMethod.Put, $"api/event/unsubscribe/{eventId}");
        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    private async Task<HttpRequestMessage> BuildRequest(HttpMethod method, string url)
    {
        var token = await _session.GetItemAsync<string>("authToken");
        var req   = new HttpRequestMessage(method, url);

        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return req;
    }
}