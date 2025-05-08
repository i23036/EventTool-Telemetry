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

builder.Services.AddScoped(sp =>
{
    var baseUrl = "https://localhost:7085"; // oder aus appsettings.json lesen
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(baseUrl)
    };
    return new ServiceClient(baseUrl, httpClient);
});


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
