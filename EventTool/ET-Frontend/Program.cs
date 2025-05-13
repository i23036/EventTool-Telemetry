using Blazored.SessionStorage;
using ET_Frontend;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddMudServices();

// 1) Lese die ApiBaseUrl aus der config
var apiUrl = builder.Configuration["ApiBaseUrl"]
             ?? throw new InvalidOperationException("Missing ApiBaseUrl in configuration.");

// 2) Registriere HttpClient mit der jeweils richtigen BaseAddress
builder.Services.AddScoped(sp =>
{
    Console.WriteLine($"Using API base URL: {apiUrl}");
    return new HttpClient { BaseAddress = new Uri(apiUrl) };
});

await builder.Build().RunAsync();