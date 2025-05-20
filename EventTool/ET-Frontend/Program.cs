using System.Net.Http.Json;
using Blazored.SessionStorage;
using ET_Frontend;
using ET_Frontend.Services.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Services.Authentication;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// === Basis-Services ===
builder.Services.AddMudServices();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<ILoginService, LoginService>();

// === Konfiguration laden ===
var environment = builder.HostEnvironment.Environment;
var configUrl = $"appsettings.{environment}.json";
var tempClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

var config = await tempClient.GetFromJsonAsync<Dictionary<string, string>>(configUrl);

if (config == null || !config.TryGetValue("ApiBaseUrl", out var apiUrl) || string.IsNullOrWhiteSpace(apiUrl))
    throw new InvalidOperationException("Missing ApiBaseUrl in configuration.");

// === AuthHandler + HttpClient einbinden ===
builder.Services.AddTransient<AuthHeaderHandler>(); // dein neuer Handler

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiUrl);
    Console.WriteLine($"Using API base URL: {apiUrl}");
}).AddHttpMessageHandler<AuthHeaderHandler>();

// === Default HttpClient für DI verfügbar machen ===
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

await builder.Build().RunAsync();