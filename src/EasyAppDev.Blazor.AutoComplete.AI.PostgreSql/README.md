# EasyAppDev.Blazor.AutoComplete.AI.PostgreSql

PostgreSQL/pgvector integration for semantic search with the Blazor AutoComplete component.

[![NuGet](https://img.shields.io/nuget/v/EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.svg)](https://www.nuget.org/packages/EasyAppDev.Blazor.AutoComplete.AI.PostgreSql/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **pgvector Support**: Native PostgreSQL vector similarity search
- **6 Distance Functions**: Cosine, L2, DotProduct + 3 pgvector-only
- **HNSW Index**: High-performance approximate nearest neighbor search
- **Self-Hosted**: Full control over your data and infrastructure

## Installation

```bash
dotnet add package EasyAppDev.Blazor.AutoComplete.AI.PostgreSql
```

## Prerequisites

1. PostgreSQL 15+ with pgvector extension:

```sql
CREATE EXTENSION vector;
```

## Quick Start

### 1. Configure Services

```csharp
// Program.cs
builder.Services.AddAutoCompletePostgreSql<Product>(
    connectionString: "Host=localhost;Database=myapp;Username=user;Password=pass",
    options => {
        options.TableName = "product_embeddings";
        options.Dimensions = 1536;
        options.DistanceFunction = DistanceFunction.Cosine;
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
| `ConnectionString` | PostgreSQL connection string | Required |
| `TableName` | Table for embeddings | `{type}_embeddings` |
| `Dimensions` | Vector dimensions | `1536` |
| `DistanceFunction` | Similarity metric | `Cosine` |
| `CreateTableIfNotExists` | Auto-create table | `true` |
| `UseHnswIndex` | Enable HNSW index | `true` |

## Distance Functions

| Function | Use Case |
|----------|----------|
| `Cosine` | Normalized embeddings (default) |
| `L2` | Euclidean distance |
| `DotProduct` | Inner product similarity |
| `L1` | Manhattan distance |
| `Hamming` | Binary vectors |
| `Jaccard` | Set similarity |

## Indexing Data

```csharp
@inject IVectorIndexer<Product> indexer

// Index all products
await indexer.IndexAsync(products);

// Re-index on update
await indexer.UpdateAsync(updatedProduct);
```

## License

MIT
