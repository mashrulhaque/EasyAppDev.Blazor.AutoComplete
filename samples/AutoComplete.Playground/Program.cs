using AutoComplete.Playground.Components;
using EasyAppDev.Blazor.AutoComplete.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Configure for running behind reverse proxy (Coolify/Nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                             | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register AutoComplete core services (IThemeManager, etc.)
builder.Services.AddAutoComplete();

// Configure OpenAI for semantic search using the simplified extension method
builder.Services.AddAutoCompleteSemanticSearch(builder.Configuration);

var app = builder.Build();

// Use forwarded headers for proper proxy support
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only redirect to HTTPS in development (Coolify handles SSL termination)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
