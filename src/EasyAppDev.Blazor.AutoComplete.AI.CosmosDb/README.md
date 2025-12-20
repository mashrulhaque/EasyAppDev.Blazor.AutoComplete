# EasyAppDev.Blazor.AutoComplete.AI.CosmosDb

Azure Cosmos DB integration for semantic search with the Blazor AutoComplete component.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI.CosmosDb/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Multi-Model Database**: NoSQL + vector search
- **Global Distribution**: Low latency worldwide
- **Integrated NoSQL**: Store documents with embeddings
- **Enterprise SLA**: 99.999% availability

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.CosmosDb
```

## Quick Start

### 1. Configure Services

```csharp
// Program.cs
builder.Services.AddAutoCompleteCosmosDb<Product>(
    endpoint: "https://myaccount.documents.azure.com:443/",
    key: "your-cosmos-key",
    databaseName: "myapp",
    containerName: "products",
    options => {
        options.VectorIndexType = VectorIndexType.DiskANN;
        options.Dimensions = 1536;
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
| `Endpoint` | Cosmos DB endpoint | Required |
| `Key` | Primary key | Required |
| `DatabaseName` | Database name | Required |
| `ContainerName` | Container name | Required |
| `VectorIndexType` | Index type | `DiskANN` |
| `Dimensions` | Vector dimensions | `1536` |
| `PartitionKeyPath` | Partition key | `/id` |

## Vector Index Types

| Type | Description |
|------|-------------|
| `Flat` | Exact search, smaller datasets |
| `QuantizedFlat` | Compressed, balanced |
| `DiskANN` | Large scale, best performance |

## Container Setup

Enable vector search on container:

```csharp
var containerProperties = new ContainerProperties("products", "/id")
{
    VectorEmbeddingPolicy = new VectorEmbeddingPolicy(
        new Collection<Embedding> {
            new Embedding {
                Path = "/embedding",
                DataType = VectorDataType.Float32,
                Dimensions = 1536,
                DistanceFunction = DistanceFunction.Cosine
            }
        })
};
```

## Integrated Document Storage

Store full documents with embeddings:

```csharp
public class ProductDocument
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float[] Embedding { get; set; }  // Auto-populated
}
```

## License

MIT
