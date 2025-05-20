using System.Net.Http.Json;
using Blazored.SessionStorage;
using ET_Frontend;
using ET_Frontend.Services.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registriert externe UI- und Speicher-Services:
// - MudBlazor für UI-Komponenten
// - Blazored SessionStorage für lokale Speicherung
// - AuthorizationCore für Zugriff auf Claims aus JWT
builder.Services.AddMudServices();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<ILoginService, LoginService>();

// Ermittelt die aktuelle Umgebung (Development, Production, ...),
// um die passende Konfigurationsdatei zu laden.
var environment = builder.HostEnvironment.Environment;

// Lädt die passende Konfigurationsdatei (z. B. appsettings.Development.json)
// und extrahiert daraus die API-Basis-URL.
var configUrl = $"appsettings.{environment}.json";
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

var config = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(configUrl);

if (config == null || !config.TryGetValue("ApiBaseUrl", out var apiUrl) || string.IsNullOrWhiteSpace(apiUrl))
{
    throw new InvalidOperationException("Missing ApiBaseUrl in configuration.");
}

// Registriert den zentralen HttpClient für alle API-Aufrufe.
// Die BaseAddress stammt aus der geladenen Konfiguration.
builder.Services.AddScoped(_ =>
{
    Console.WriteLine($"Using API base URL: {apiUrl}");
    return new HttpClient { BaseAddress = new Uri(apiUrl, UriKind.RelativeOrAbsolute) };
});

await builder.Build().RunAsync();