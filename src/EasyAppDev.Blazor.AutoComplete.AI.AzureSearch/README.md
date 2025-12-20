# EasyAppDev.Blazor.AutoComplete.AI.AzureSearch

Azure AI Search integration for semantic search with the Blazor AutoComplete component.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI.AzureSearch/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Hybrid Search**: Combines vector + keyword search
- **Semantic Ranking**: AI-powered result re-ranking
- **Managed Service**: No infrastructure to maintain
- **Enterprise Ready**: Built-in security, scaling, and SLA

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.AzureSearch
```

## Quick Start

### 1. Configure Services

```csharp
// Program.cs
builder.Services.AddAutoCompleteAzureSearch<Product>(
    endpoint: "https://mysearch.search.windows.net",
    apiKey: "your-admin-key",
    indexName: "products",
    options => {
        options.EnableSemanticRanking = true;
        options.SemanticConfigurationName = "my-semantic-config";
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());

// Register embedding generator
builder.Services.AddAutoCompleteVectorSearch<Product>(
    openAiApiKey: "sk-...");
```

### 2. Use the Component

```razor
@using EasyAppDev.Blazor.AutoComplete.AI

<VectorAutoComplete TItem="Product"
                    TextField="@(p => p.Name)"
                    @bind-Value="@selectedProduct"
                    Placeholder="Semantic search..." />
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `Endpoint` | Azure Search endpoint | Required |
| `ApiKey` | Admin API key | Required |
| `IndexName` | Search index name | Required |
| `EnableSemanticRanking` | Use semantic ranker | `false` |
| `VectorFieldName` | Vector field in index | `embedding` |
| `TopK` | Max results to return | `10` |

## Hybrid Search

Azure AI Search combines vector similarity with traditional keyword matching:

```csharp
options.EnableHybridSearch = true;
options.HybridSearchWeight = 0.5f; // 50% vector, 50% keyword
```

## Creating an Index

Use Azure Portal or SDK to create an index with a vector field:

```json
{
  "name": "products",
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true },
    { "name": "name", "type": "Edm.String", "searchable": true },
    { "name": "embedding", "type": "Collection(Edm.Single)", "dimensions": 1536, "vectorSearchProfile": "default" }
  ]
}
```

## License

MIT
