using System.Text.Json;
using Microsoft.Extensions.VectorData;

namespace EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

/// <summary>
/// Record type for storing items with their embeddings in Azure AI Search.
/// The TItem is serialized as JSON for flexible storage.
/// Note: Must be public for Azure Search SDK JSON serialization to work.
/// </summary>
public sealed class AzureSearchVectorRecord
{
    /// <summary>
    /// Unique document ID.
    /// </summary>
    [VectorStoreKey]
    public required string Id { get; init; }

    /// <summary>
    /// JSON serialized item data.
    /// </summary>
    [VectorStoreData]
    public required string ItemJson { get; init; }

    /// <summary>
    /// Text content for keyword search.
    /// </summary>
    [VectorStoreData]
    public required string Content { get; init; }

    /// <summary>
    /// Title field for display and filtering.
    /// </summary>
    [VectorStoreData]
    public string? Title { get; init; }

    /// <summary>
    /// The embedding vector.
    /// Dimensions are configured at runtime.
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
    /// Creates an AzureSearchVectorRecord from an item.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="content">Text content for full-text search.</param>
    /// <param name="title">Optional title for filtering.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <returns>A new AzureSearchVectorRecord instance.</returns>
    public static AzureSearchVectorRecord Create<TItem>(
        string id,
        TItem item,
        string content,
        string? title,
        ReadOnlyMemory<float> embedding)
    {
        return new AzureSearchVectorRecord
        {
            Id = id,
            ItemJson = JsonSerializer.Serialize(item),
            Content = content,
            Title = title,
            Embedding = embedding
        };
    }
}
