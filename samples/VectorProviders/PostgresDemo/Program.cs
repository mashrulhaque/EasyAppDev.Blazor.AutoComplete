using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Extensions;
using PostgresDemo.Components;
using PostgresDemo.Models;
using PostgresDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure PostgreSQL vector search
// NOTE: Set your OpenAI API key in appsettings.json or environment variables
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("OpenAI:ApiKey is required. Set it in appsettings.json or environment variables.");

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Database=vectordemo;Username=postgres;Password=postgres";

// Register PostgreSQL provider with both search and indexer
builder.Services.AddAutoCompletePostgres<Product>(
    configureOptions: options =>
    {
        options.ConnectionString = connectionString;
        options.CollectionName = "products";
        options.EmbeddingDimensions = 1536;  // text-embedding-3-small
    },
    textSelector: p => $"{p.Name} {p.Description} {p.Category}",
    idSelector: p => p.Id.ToString());

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
