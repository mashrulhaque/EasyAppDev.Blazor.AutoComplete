using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;

/// <summary>
/// Configuration options for PostgreSQL vector search provider.
/// </summary>
public class PostgresVectorSearchOptions
{
    /// <summary>
    /// PostgreSQL connection string.
    /// Must include a database with pgvector extension installed.
    /// </summary>
    /// <example>Host=localhost;Database=myapp;Username=postgres;Password=secret</example>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Name of the vector collection (table) to use.
    /// Will be created if it doesn't exist when using the indexer.
    /// </summary>
    public string CollectionName { get; set; } = "";

    /// <summary>
    /// Dimension of the embedding vectors. Default: 1536 (OpenAI text-embedding-3-small).
    /// Must match the embedding model's output dimension.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// Distance function for similarity search. Default: Cosine.
    /// pgvector supports: Cosine, Euclidean (L2), DotProduct (InnerProduct), Manhattan (L1).
    /// </summary>
    public DistanceFunction DistanceFunction { get; set; } = DistanceFunction.Cosine;

    /// <summary>
    /// Schema name for the collection. Default: public.
    /// Note: This option is for reference when configuring PostgresVectorStore externally.
    /// The Semantic Kernel PostgreSQL connector uses the default schema.
    /// </summary>
    public string Schema { get; set; } = "public";

    /// <summary>
    /// Batch size for indexing operations. Default: 100.
    /// Larger batches are faster but use more memory.
    /// </summary>
    public int IndexBatchSize { get; set; } = 100;

    /// <summary>
    /// Whether to create an HNSW index for faster approximate search.
    /// Default: true. Set to false for exact search on small datasets.
    /// Note: Configure HNSW index via PostgreSQL/pgvector directly after collection creation.
    /// The Semantic Kernel connector creates a basic collection without indexes.
    /// </summary>
    public bool CreateHnswIndex { get; set; } = true;

    /// <summary>
    /// HNSW index parameter: max connections per layer. Default: 16.
    /// Higher values = better recall, slower builds, more memory.
    /// Note: Apply via PostgreSQL after collection creation:
    /// CREATE INDEX ON collection USING hnsw (embedding vector_cosine_ops) WITH (m = 16, ef_construction = 64);
    /// </summary>
    public int HnswM { get; set; } = 16;

    /// <summary>
    /// HNSW index parameter: size of dynamic candidate list. Default: 64.
    /// Higher values = better recall during index building.
    /// Note: Apply via PostgreSQL after collection creation (see HnswM).
    /// </summary>
    public int HnswEfConstruction { get; set; } = 64;
}
