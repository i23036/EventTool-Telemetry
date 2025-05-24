using Blazored.SessionStorage;
using ET_Frontend;
using ET_Frontend.Services.ApiClients;
using ET_Frontend.Services.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Services.Authentication;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// === Root-Komponenten ===
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// === Grundlegende UI- und Authentifizierungsdienste ===
builder.Services.AddMudServices();                          // MudBlazor
builder.Services.AddBlazoredSessionStorage();               // SessionStorage für Tokens etc.
builder.Services.AddAuthorizationCore();                    // Blazor Auth-System
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<ILoginService, LoginService>();

// === Konfiguration laden (z. B. appsettings.Production.json) ===
var env = builder.HostEnvironment.Environment;
var configUrl = $"appsettings.{env}.json";
var tempClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await tempClient.GetFromJsonAsync<Dictionary<string, string>>(configUrl);

if (config is null || !config.TryGetValue("ApiBaseUrl", out var apiUrl) || string.IsNullOrWhiteSpace(apiUrl))
    throw new InvalidOperationException("Missing 'ApiBaseUrl' in configuration.");

// === HTTP-Handler für Auth (z. B. Bearer-Token automatisch hinzufügen) ===
builder.Services.AddTransient<AuthHeaderHandler>();

// === Named HttpClient "ApiClient" ===
// Dieser verwendet den AuthHandler und die dynamisch geladene API-URL
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiUrl);
    Console.WriteLine($"[HttpClient] Using API base URL: {apiUrl}");
}).AddHttpMessageHandler<AuthHeaderHandler>();  // Token wird automatisch hinzugefügt

// === Default HttpClient setzen (wird bei @inject HttpClient verwendet) ===
// Intern wird hier „ApiClient“ per Factory aufgelöst
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

// === API-Client-Wrapper (für User, Events etc.) ===
builder.Services.AddScoped<IUserApi, UserApi>();
// builder.Services.AddScoped<IEventApi, EventApi>(); // weitere API-Clients hier registrieren

await builder.Build().RunAsync();