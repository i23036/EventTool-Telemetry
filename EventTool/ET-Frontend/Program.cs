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

var apiBase = new Uri("https://localhost:7085/");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBase });

await builder.Build().RunAsync();