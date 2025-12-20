# EasyAppDev.Blazor.AutoComplete.AI.Qdrant

Qdrant integration for semantic search with the Blazor AutoComplete component.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.AI.Qdrant.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI.Qdrant/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Open Source**: Self-hosted or Qdrant Cloud
- **Advanced Filtering**: Rich query language
- **Multiple Distance Metrics**: Cosine, Euclidean, Dot Product
- **Payload Storage**: Store metadata with vectors

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.Qdrant
```

## Quick Start

### 1. Start Qdrant (Docker)

```bash
docker run -p 6333:6333 qdrant/qdrant
```

### 2. Configure Services

```csharp
// Program.cs
builder.Services.AddAutoCompleteQdrant<Product>(
    host: "localhost",
    port: 6333,
    collectionName: "products",
    options => {
        options.VectorSize = 1536;
        options.DistanceMetric = Distance.Cosine;
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());

// Register embedding generator
builder.Services.AddAutoCompleteVectorSearch<Product>(
    openAiApiKey: "sk-...");
```

### 3. Use the Component

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
| `Host` | Qdrant server host | `localhost` |
| `Port` | Qdrant server port | `6333` |
| `CollectionName` | Collection name | Required |
| `VectorSize` | Vector dimensions | `1536` |
| `DistanceMetric` | Similarity metric | `Cosine` |
| `ApiKey` | API key (Cloud) | `null` |

## Qdrant Cloud

For managed Qdrant:

```csharp
builder.Services.AddAutoCompleteQdrant<Product>(
    host: "xyz.qdrant.io",
    port: 6333,
    collectionName: "products",
    options => {
        options.ApiKey = "your-qdrant-cloud-api-key";
        options.UseTls = true;
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());
```

## Filtering

Filter by payload fields:

```csharp
options.Filter = new Filter {
    Must = new[] {
        new FieldCondition("category", new MatchValue("Electronics"))
    }
};
```

## Distance Metrics

| Metric | Use Case |
|--------|----------|
| `Cosine` | Normalized embeddings (default) |
| `Euclidean` | Absolute distances |
| `DotProduct` | Non-normalized vectors |

## License

MIT
