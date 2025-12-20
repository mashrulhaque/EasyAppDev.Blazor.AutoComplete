using EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Extensions;
using QdrantDemo.Components;
using QdrantDemo.Models;
using QdrantDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Get configuration
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("OpenAI:ApiKey is required. Set it in appsettings.json or environment variables.");

var qdrantHost = builder.Configuration["Qdrant:Host"] ?? "localhost";
var qdrantPort = int.Parse(builder.Configuration["Qdrant:Port"] ?? "6334");

// Register Qdrant provider with both search and indexer
// Note: Qdrant uses Guid for IDs, so we create a deterministic Guid from the int Id
builder.Services.AddAutoCompleteQdrant<Product>(
    configureOptions: options =>
    {
        options.Host = qdrantHost;
        options.Port = qdrantPort;
        options.CollectionName = "products";
        options.EmbeddingDimensions = 1536;  // text-embedding-3-small
    },
    textSelector: p => $"{p.Name} {p.Description} {p.Category}",
    idSelector: p => new Guid(p.Id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));

// Register embedding generator and vector search data source
builder.Services.AddAutoCompleteVectorSearch<Product>(
    openAiApiKey: openAiApiKey,
    configureOptions: options =>
    {
        options.MaxResults = 10;
        options.MinSimilarityScore = 0.15f;
    });

// Add background indexing service
builder.Services.AddHostedService<ProductIndexingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
