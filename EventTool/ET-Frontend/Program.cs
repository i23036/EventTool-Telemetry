using System.Linq;
using System.Net.Http.Json;
using Blazored.SessionStorage;
using ET_Frontend;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddMudServices();

// 1. Hole Umgebung (Development / Production etc.)
var environment = builder.HostEnvironment.Environment;

// 2. Konfigurationsdatei laden (z. B. wwwroot/appsettings.Development.json)
var configUrl = $"appsettings.{environment}.json";
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

var config = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(configUrl);

if (config == null || !config.TryGetValue("ApiBaseUrl", out var apiUrl) || string.IsNullOrWhiteSpace(apiUrl))
{
    throw new InvalidOperationException("Missing ApiBaseUrl in configuration.");
}

// 3. HttpClient registrieren
builder.Services.AddScoped(_ =>
{
    Console.WriteLine($"Using API base URL: {apiUrl}");
    return new HttpClient { BaseAddress = new Uri(apiUrl, UriKind.RelativeOrAbsolute) };
});

await builder.Build().RunAsync();