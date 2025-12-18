using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EasyAppDev.Blazor.AutoComplete.Extensions;
using TrimTest.Wasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register AutoComplete services (IThemeManager, IAutoCompleteServiceFactory)
builder.Services.AddAutoComplete();

await builder.Build().RunAsync();
