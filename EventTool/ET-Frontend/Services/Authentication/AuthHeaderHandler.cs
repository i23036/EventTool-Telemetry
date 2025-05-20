using System.Net.Http.Headers;
using Blazored.SessionStorage;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ISessionStorageService _sessionStorage;
    private const string TokenKey = "authToken";

    public AuthHeaderHandler(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _sessionStorage.GetItemAsStringAsync(TokenKey);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}