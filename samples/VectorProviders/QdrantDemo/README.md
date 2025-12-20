# Qdrant Vector Search Demo

This sample demonstrates using Qdrant (open-source vector database) for semantic search in Blazor AutoComplete.

## Prerequisites

1. **Docker** (for running Qdrant locally)
2. **OpenAI API key** for generating embeddings

## Setup

### 1. Start Qdrant with Docker

```bash
cd samples/VectorProviders/QdrantDemo
docker-compose up -d
```

This starts Qdrant with:
- REST API on port 6333
- gRPC API on port 6334 (used by the provider)
- Persistent storage in a Docker volume

Verify Qdrant is running:
```bash
curl http://localhost:6333/healthz
```

### 2. Configure API Key

Set your OpenAI API key in `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key-here"
  }
}
```

Or use environment variables:

```bash
export OpenAI__ApiKey="sk-your-api-key-here"
```

### 3. Run the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

## How It Works

1. **On startup**, the background service creates a Qdrant collection and indexes sample products
2. **Qdrant** stores embeddings as vectors with associated payloads (metadata)
3. **Search queries** are converted to embeddings using OpenAI
4. **HNSW index** enables fast approximate nearest neighbor search

## Qdrant Features

### HNSW Index

Qdrant uses Hierarchical Navigable Small World (HNSW) graphs for fast ANN search:

- **Sub-millisecond latency** for similarity search
- **Scalable** to billions of vectors
- **High recall** with configurable accuracy/speed tradeoffs

### Payload Filtering

Combine vector search with metadata filtering:

```csharp
// Example: Search within a specific category
options.Filter = new { Category = "Electronics" };
```

### Distance Functions

Supported distance metrics:
- `Cosine` (default) - Best for normalized embeddings
- `Euclidean` (L2) - When magnitude matters
- `DotProduct` - For normalized vectors

## Configuration Options

```csharp
builder.Services.AddAutoCompleteQdrant<Product>(
    configureOptions: options =>
    {
        options.Host = "localhost";
        options.Port = 6334;                      // gRPC port
        options.ApiKey = null;                    // Optional for local
        options.CollectionName = "products";
        options.EmbeddingDimensions = 1536;
        options.DistanceFunction = DistanceFunction.Cosine;
        options.IndexBatchSize = 100;
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());
```

## Qdrant Dashboard

Access the Qdrant dashboard at `http://localhost:6333/dashboard` to:
- Browse collections
- View indexed vectors
- Run test queries
- Monitor performance

## Stopping Qdrant

```bash
docker-compose down

# To also remove the data volume:
docker-compose down -v
```

## Production Deployment

For production, consider:
- **Qdrant Cloud** - Managed service with automatic scaling
- **Kubernetes** - Self-hosted with the official Helm chart
- **Multiple nodes** - Distributed mode for high availability

See [Qdrant documentation](https://qdrant.tech/documentation/) for more details.
