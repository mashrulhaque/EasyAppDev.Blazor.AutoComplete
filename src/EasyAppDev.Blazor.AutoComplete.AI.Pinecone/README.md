# EasyAppDev.Blazor.AutoComplete.AI.Pinecone

Pinecone integration for semantic search with the Blazor AutoComplete component.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.AI.Pinecone.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI.Pinecone/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Serverless**: No infrastructure to manage
- **Namespaces**: Organize vectors by category
- **Automatic Scaling**: Handles any volume
- **Low Latency**: Optimized for real-time search

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.Pinecone
```

## Quick Start

### 1. Configure Services

```csharp
// Program.cs
builder.Services.AddAutoCompletePinecone<Product>(
    apiKey: "your-pinecone-api-key",
    environment: "us-east-1-aws",
    indexName: "products",
    options => {
        options.Namespace = "production";
        options.TopK = 10;
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
| `ApiKey` | Pinecone API key | Required |
| `Environment` | Pinecone environment | Required |
| `IndexName` | Index name | Required |
| `Namespace` | Vector namespace | `""` (default) |
| `TopK` | Max results | `10` |
| `IncludeMetadata` | Return metadata | `true` |

## Namespaces

Organize vectors by category or tenant:

```csharp
options.Namespace = "category-electronics";
// or
options.Namespace = $"tenant-{tenantId}";
```

## Metadata Filtering

Filter results by metadata:

```csharp
options.Filter = new Dictionary<string, object> {
    { "category", "Electronics" },
    { "inStock", true }
};
```

## Creating an Index

Use Pinecone console or SDK:

```python
import pinecone

pinecone.create_index(
    name="products",
    dimension=1536,
    metric="cosine"
)
```

## License

MIT
