using System.Net.Http.Headers;
using Blazored.SessionStorage;
using ET_Frontend.Helpers;

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

    private async Task<HttpRequestMessage> BuildRequest(HttpMethod method, string url)
    {
        var token = await _session.GetItemAsync<string>("authToken");
        var req   = new HttpRequestMessage(method, url);

        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return req;
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
}