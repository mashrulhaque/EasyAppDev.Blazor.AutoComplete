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

// Register HttpClient for OData demo
builder.Services.AddHttpClient();

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

// OData endpoint for TimeZone demo - supports $filter, $top, $select, $orderby
app.MapGet("/odata/timezones", (HttpContext context) =>
{
    var query = TimeZoneInfo.GetSystemTimeZones().AsQueryable();

    // Manual OData-like query handling for minimal APIs
    var queryParams = context.Request.Query;

    // Handle $filter
    if (queryParams.TryGetValue("$filter", out var filter))
    {
        var filterStr = filter.ToString().ToLower();

        // Parse startswith(DisplayName,'value') or startswith(StandardName,'value')
        if (filterStr.Contains("startswith"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(filterStr, @"startswith\(tolower\((\w+)\)\s*,\s*'([^']+)'\)");
            if (match.Success)
            {
                var field = match.Groups[1].Value.ToLower();
                var value = match.Groups[2].Value;

                query = field switch
                {
                    "displayname" => query.Where(tz => tz.DisplayName.ToLower().StartsWith(value)),
                    "standardname" => query.Where(tz => tz.StandardName.ToLower().StartsWith(value)),
                    _ => query
                };
            }
        }
        // Parse contains(DisplayName,'value') or contains(StandardName,'value')
        else if (filterStr.Contains("contains"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(filterStr, @"contains\(tolower\((\w+)\)\s*,\s*'([^']+)'\)");
            if (match.Success)
            {
                var field = match.Groups[1].Value.ToLower();
                var value = match.Groups[2].Value;

                query = field switch
                {
                    "displayname" => query.Where(tz => tz.DisplayName.ToLower().Contains(value)),
                    "standardname" => query.Where(tz => tz.StandardName.ToLower().Contains(value)),
                    _ => query
                };
            }

            // Handle OR filters: contains(tolower(field1),'value') or contains(tolower(field2),'value')
            var orMatch = System.Text.RegularExpressions.Regex.Match(filterStr, @"\(contains\(tolower\((\w+)\)\s*,\s*'([^']+)'\)\s+or\s+contains\(tolower\((\w+)\)\s*,\s*'([^']+)'\)\)");
            if (orMatch.Success)
            {
                var field1 = orMatch.Groups[1].Value.ToLower();
                var value1 = orMatch.Groups[2].Value;
                var field2 = orMatch.Groups[3].Value.ToLower();
                var value2 = orMatch.Groups[4].Value;

                query = query.Where(tz =>
                    (field1 == "displayname" && tz.DisplayName.ToLower().Contains(value1)) ||
                    (field1 == "standardname" && tz.StandardName.ToLower().Contains(value1)) ||
                    (field2 == "displayname" && tz.DisplayName.ToLower().Contains(value2)) ||
                    (field2 == "standardname" && tz.StandardName.ToLower().Contains(value2)));
            }
        }
    }

    // Handle $top
    if (queryParams.TryGetValue("$top", out var topStr) && int.TryParse(topStr, out var top))
    {
        query = query.Take(top);
    }
    else
    {
        query = query.Take(100); // Default limit
    }

    // Return as simplified DTOs
    var results = query.Select(tz => new
    {
        tz.Id,
        tz.DisplayName,
        tz.StandardName,
        tz.BaseUtcOffset
    }).ToList();

    return Results.Json(results);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
