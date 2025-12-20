# PostgreSQL Vector Search Demo

This sample demonstrates using PostgreSQL with pgvector for semantic search in Blazor AutoComplete.

## Prerequisites

1. **PostgreSQL 15+** with pgvector extension
2. **OpenAI API key** for generating embeddings

## Setup

### 1. Start PostgreSQL with Docker

```bash
docker run -d \
  --name postgres-vector \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=vectordemo \
  -p 5432:5432 \
  pgvector/pgvector:pg16
```

Or use the provided docker-compose file in the parent directory.

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
cd samples/VectorProviders/PostgresDemo
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

## How It Works

1. **On startup**, the background service indexes sample products into PostgreSQL
2. **The pgvector extension** stores embeddings as vector columns
3. **Search queries** are converted to embeddings using OpenAI
4. **pgvector** performs fast cosine similarity search

## Sample Queries

Try these semantic searches:
- "laptop for software development"
- "cooking appliances for healthy meals"
- "outdoor camping gear"
- "books about programming best practices"
- "gift for a fitness enthusiast"

## Configuration Options

```csharp
builder.Services.AddAutoCompletePostgres<Product>(
    configureOptions: options =>
    {
        options.ConnectionString = "Host=localhost;Database=vectordemo;...";
        options.CollectionName = "products";           // Table name
        options.EmbeddingDimensions = 1536;            // text-embedding-3-small
        options.DistanceFunction = DistanceFunction.Cosine;
        options.IndexBatchSize = 100;
    },
    textSelector: p => $"{p.Name} {p.Description}",
    idSelector: p => p.Id.ToString());
```

## pgvector Distance Functions

PostgreSQL with pgvector supports these distance functions:

| Function | Description | Use Case |
|----------|-------------|----------|
| `Cosine` | Cosine similarity (default) | General-purpose semantic search |
| `L2` | Euclidean distance | When magnitude matters |
| `DotProduct` | Inner product | Normalized embeddings |
| `Manhattan` | L1 distance | pgvector-specific |
| `Hamming` | Hamming distance | Binary vectors |
| `Jaccard` | Jaccard distance | Set similarity |
