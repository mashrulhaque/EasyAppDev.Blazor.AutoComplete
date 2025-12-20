# Vector Provider Samples

This directory contains sample applications demonstrating each vector database provider for the AutoComplete AI semantic search feature.

## Available Samples

| Sample | Database | Description |
|--------|----------|-------------|
| [PostgresDemo](./PostgresDemo/) | PostgreSQL + pgvector | Self-hosted, HNSW index, 6 distance functions |
| [AzureSearchDemo](./AzureSearchDemo/) | Azure AI Search | Hybrid search (vector + keyword), managed service |
| [QdrantDemo](./QdrantDemo/) | Qdrant | Open-source, HNSW, payload filtering |

## Quick Start

### PostgreSQL (Self-Hosted)

```bash
# Start PostgreSQL with pgvector
docker run -d --name postgres-vector \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=vectordemo \
  -p 5432:5432 pgvector/pgvector:pg16

# Run the demo
cd PostgresDemo
dotnet run
```

### Qdrant (Self-Hosted)

```bash
cd QdrantDemo

# Start Qdrant
docker-compose up -d

# Run the demo
dotnet run
```

### Azure AI Search (Cloud)

Requires Azure subscription:
1. Create Azure AI Search service (Basic tier+)
2. Create Azure OpenAI service with embedding model
3. Configure settings in `appsettings.json`
4. Run `dotnet run`

## Prerequisites

All samples require:
- .NET 9.0 SDK
- OpenAI API key (or Azure OpenAI for Azure samples)
- Docker (for PostgreSQL and Qdrant)

## Configuration

Each sample uses `appsettings.json` for configuration. You can also use environment variables:

```bash
# OpenAI
export OpenAI__ApiKey="sk-your-api-key"

# PostgreSQL
export ConnectionStrings__Postgres="Host=localhost;Database=vectordemo;..."

# Qdrant
export Qdrant__Host="localhost"
export Qdrant__Port="6334"

# Azure
export AzureSearch__Endpoint="https://..."
export AzureSearch__ApiKey="..."
export AzureOpenAI__Endpoint="https://..."
export AzureOpenAI__ApiKey="..."
```

## Architecture

All samples follow the same architecture:

```
Program.cs                 # Service registration
Models/Product.cs          # Data model
Services/ProductDataService.cs      # Sample data
Services/ProductIndexingService.cs  # Background indexer
Components/Pages/Home.razor         # UI with SemanticAutoComplete
```

## Adding a New Provider Sample

1. Copy an existing sample directory
2. Update namespace and project references
3. Modify `Program.cs` to use the new provider
4. Update the README with provider-specific setup

## Related Documentation

- [Migration Guide](../../docs/migration-guide.md) - Migrating from in-memory to vector providers
- [Troubleshooting Guide](../../docs/troubleshooting.md) - Common issues and solutions
- [Performance Benchmarks](../../docs/benchmarks.md) - Provider comparison
