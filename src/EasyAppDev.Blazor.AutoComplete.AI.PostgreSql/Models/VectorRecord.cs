using System.Text.Json;
using Microsoft.Extensions.VectorData;

namespace EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;

/// <summary>
/// Internal record type for storing items with their embeddings in PostgreSQL.
/// The TItem is serialized as JSON for flexible storage.
/// </summary>
internal sealed class VectorRecord
{
    /// <summary>
    /// Unique identifier for the record.
    /// </summary>
    [VectorStoreKey]
    public required string Id { get; init; }

    /// <summary>
    /// JSON serialized item data.
    /// </summary>
    [VectorStoreData]
    public required string ItemJson { get; init; }

    /// <summary>
    /// Text representation used for embedding generation.
    /// </summary>
    [VectorStoreData]
    public required string Text { get; init; }

    /// <summary>
    /// The embedding vector.
    /// Dimensions are configured at runtime via VectorStoreRecordDefinition.
    /// </summary>
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public required ReadOnlyMemory<float> Embedding { get; init; }

    /// <summary>
    /// Deserializes the item from JSON.
    /// </summary>
    /// <typeparam name="TItem">The item type to deserialize to.</typeparam>
    /// <returns>The deserialized item, or null if deserialization fails.</returns>
    public TItem? GetItem<TItem>()
    {
        if (string.IsNullOrEmpty(ItemJson))
            return default;

        return JsonSerializer.Deserialize<TItem>(ItemJson);
    }

    /// <summary>
    /// Creates a VectorRecord from an item.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="text">Text representation for embedding.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <returns>A new VectorRecord instance.</returns>
    public static VectorRecord Create<TItem>(
        string id,
        TItem item,
        string text,
        ReadOnlyMemory<float> embedding)
    {
        return new VectorRecord
        {
            Id = id,
            ItemJson = JsonSerializer.Serialize(item),
            Text = text,
            Embedding = embedding
        };
    }
}
