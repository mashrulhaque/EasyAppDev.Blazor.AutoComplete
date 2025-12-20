# EasyAppDev.Blazor.AutoComplete.AI

AI-powered semantic search for the Blazor AutoComplete component. Search by meaning, not just keywords.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.AI.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EasyAppDev.Blazor.AutoComplete.AI.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Semantic Search**: Understands meaning, synonyms, and concepts
- **SIMD Acceleration**: 3-5x faster similarity calculations
- **Dual Caching**: Separate item/query caches with LRU eviction
- **Multiple Providers**: OpenAI, Azure OpenAI, Ollama, or custom
- **Hybrid Search**: Falls back to text matching for short queries

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI
```

> Note: This package includes the core AutoComplete component as a dependency.

## Quick Start

### 1. Configure OpenAI

**Option A: appsettings.json (Recommended)**

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-...",
    "Model": "text-embedding-3-small"
  }
}
```

```csharp
// Program.cs
builder.Services.AddAutoCompleteSemanticSearch(builder.Configuration);
```

**Option B: Direct API Key**

```csharp
builder.Services.AddAutoCompleteSemanticSearch(
    apiKey: "sk-proj-...",
    model: "text-embedding-3-small");
```

### 2. Use the Component

```razor
@using EasyAppDev.Blazor.AutoComplete.AI

<SemanticAutoComplete TItem="Document"
                      Items="@docs"
                      SearchFields="@(d => new[] { d.Title, d.Description })"
                      TextField="@(d => d.Title)"
                      SimilarityThreshold="0.15"
                      @bind-Value="@selectedDoc"
                      Placeholder="Search by meaning..." />
```

## Supported Providers

| Provider | Setup Method | Cost | Privacy |
|----------|--------------|------|---------|
| **OpenAI** | `AddAutoCompleteSemanticSearch()` | $ | Cloud |
| **Azure OpenAI** | `AddAutoCompleteSemanticSearchWithAzure()` | $ | Azure |
| **Ollama** | Custom `IEmbeddingGenerator` | Free | Local |

### Azure OpenAI

```csharp
builder.Services.AddAutoCompleteSemanticSearchWithAzure(
    endpoint: "https://my-resource.openai.azure.com/",
    apiKey: "your-azure-api-key",
    deploymentName: "text-embedding-ada-002");
```

### Ollama (Local)

```csharp
using OllamaSharp;

var ollamaClient = new OllamaApiClient("http://localhost:11434");
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
    ollamaClient.AsEmbeddingGenerator("nomic-embed-text"));
```

## Advanced Configuration

```razor
<SemanticAutoComplete TItem="Product"
                      Items="@products"
                      SearchFields="@(p => new[] { p.Name, p.Description })"

                      @* Cache Configuration *@
                      ItemCacheDuration="TimeSpan.FromHours(2)"
                      QueryCacheDuration="TimeSpan.FromMinutes(30)"
                      MaxItemCacheSize="20000"

                      @* Pre-warming *@
                      PreWarmCache="true"
                      ShowCacheStatus="true"

                      @* Search Tuning *@
                      SimilarityThreshold="0.15"
                      MinSearchLength="3"
                      DebounceMs="500"

                      @bind-Value="@selectedProduct" />
```

## Vector Database Providers

For production-grade semantic search with persistent storage, use vector database providers:

| Package | Provider |
|---------|----------|
| `EasyAppDev.Blazor.AutoComplete.AI.PostgreSql` | PostgreSQL/pgvector |
| `EasyAppDev.Blazor.AutoComplete.AI.AzureSearch` | Azure AI Search |
| `EasyAppDev.Blazor.AutoComplete.AI.Pinecone` | Pinecone |
| `EasyAppDev.Blazor.AutoComplete.AI.Qdrant` | Qdrant |
| `EasyAppDev.Blazor.AutoComplete.AI.CosmosDb` | Azure CosmosDB |

## Performance

- **SIMD Acceleration**: 3-5x faster cosine similarity
- **Cache Hit Rate**: 80%+ after warm-up
- **API Cost Reduction**: 80%+ due to caching

## License

MIT
