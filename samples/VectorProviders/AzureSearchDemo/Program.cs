using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Extensions;
using AzureSearchDemo.Components;
using AzureSearchDemo.Models;
using AzureSearchDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Get configuration
var azureSearchEndpoint = builder.Configuration["AzureSearch:Endpoint"]
    ?? throw new InvalidOperationException("AzureSearch:Endpoint is required.");

var azureSearchApiKey = builder.Configuration["AzureSearch:ApiKey"]
    ?? throw new InvalidOperationException("AzureSearch:ApiKey is required.");

var azureOpenAiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required.");

var azureOpenAiApiKey = builder.Configuration["AzureOpenAI:ApiKey"]
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is required.");

var azureOpenAiDeployment = builder.Configuration["AzureOpenAI:DeploymentName"]
    ?? "text-embedding-ada-002";

// Register Azure AI Search provider with both search and indexer
builder.Services.AddAutoCompleteAzureSearch<Product>(
    configureOptions: options =>
    {
        options.Endpoint = azureSearchEndpoint;
        options.ApiKey = azureSearchApiKey;
        options.IndexName = "products";
        options.EnableHybridSearch = true;  // Vector + keyword search
        options.EmbeddingDimensions = 1536;
    },
    textSelector: p => $"{p.Name} {p.Description} {p.Category}",
    idSelector: p => p.Id.ToString());

// Register Azure OpenAI embedding generator and vector search data source
builder.Services.AddAutoCompleteVectorSearchWithAzure<Product>(
    endpoint: azureOpenAiEndpoint,
    apiKey: azureOpenAiApiKey,
    deploymentName: azureOpenAiDeployment,
    configureOptions: options =>
    {
        options.MaxResults = 10;
        options.MinSimilarityScore = 0.15f;
        options.EnableHybridSearch = true;
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
