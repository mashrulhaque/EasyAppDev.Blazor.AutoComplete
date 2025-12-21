using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.VectorData;

namespace EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

/// <summary>
/// Internal record type for storing items with their embeddings in Azure CosmosDB.
/// The TItem is serialized as JSON for flexible storage.
/// </summary>
/// <remarks>
/// For AOT compatibility, use the overloads that accept <see cref="JsonSerializerOptions"/>
/// or <see cref="JsonTypeInfo{T}"/> with a source-generated context.
/// </remarks>
internal sealed class CosmosDbVectorRecord
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
    /// Dimensions are configured at runtime via VectorStoreCollectionDefinition.
    /// </summary>
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public required ReadOnlyMemory<float> Embedding { get; init; }

    /// <summary>
    /// Deserializes the item from JSON.
    /// </summary>
    /// <typeparam name="TItem">The item type to deserialize to.</typeparam>
    /// <returns>The deserialized item, or default if deserialization fails.</returns>
    /// <remarks>
    /// This method uses reflection-based deserialization and is NOT AOT-compatible.
    /// For AOT scenarios, use <see cref="GetItem{TItem}(JsonSerializerOptions)"/> or
    /// <see cref="GetItem{TItem}(JsonTypeInfo{TItem})"/> instead.
    /// </remarks>
    public TItem? GetItem<TItem>()
    {
        return GetItemCore<TItem>(null, null);
    }

    /// <summary>
    /// Deserializes the item from JSON using the specified options.
    /// </summary>
    /// <typeparam name="TItem">The item type to deserialize to.</typeparam>
    /// <param name="options">JSON serializer options with a configured TypeInfoResolver for AOT compatibility.</param>
    /// <returns>The deserialized item, or default if deserialization fails.</returns>
    public TItem? GetItem<TItem>(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return GetItemCore<TItem>(options, null);
    }

    /// <summary>
    /// Deserializes the item from JSON using the specified type info.
    /// </summary>
    /// <typeparam name="TItem">The item type to deserialize to.</typeparam>
    /// <param name="jsonTypeInfo">The JSON type info from a source-generated context.</param>
    /// <returns>The deserialized item, or default if deserialization fails.</returns>
    public TItem? GetItem<TItem>(JsonTypeInfo<TItem> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        return GetItemCore<TItem>(null, jsonTypeInfo);
    }

    private TItem? GetItemCore<TItem>(JsonSerializerOptions? options, JsonTypeInfo<TItem>? jsonTypeInfo)
    {
        if (string.IsNullOrEmpty(ItemJson))
            return default;

        try
        {
            if (jsonTypeInfo is not null)
                return JsonSerializer.Deserialize(ItemJson, jsonTypeInfo);

            if (options is not null)
                return JsonSerializer.Deserialize<TItem>(ItemJson, options);

            return JsonSerializer.Deserialize<TItem>(ItemJson);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Creates a CosmosDbVectorRecord from an item.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="text">Text representation for embedding.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <returns>A new CosmosDbVectorRecord instance.</returns>
    /// <remarks>
    /// This method uses reflection-based serialization and is NOT AOT-compatible.
    /// For AOT scenarios, use the overloads that accept JsonSerializerOptions or JsonTypeInfo.
    /// </remarks>
    public static CosmosDbVectorRecord Create<TItem>(
        string id,
        TItem item,
        string text,
        ReadOnlyMemory<float> embedding)
    {
        return CreateCore(id, item, text, embedding, null, null);
    }

    /// <summary>
    /// Creates a CosmosDbVectorRecord from an item using the specified options.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="text">Text representation for embedding.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <param name="options">JSON serializer options with a configured TypeInfoResolver for AOT compatibility.</param>
    /// <returns>A new CosmosDbVectorRecord instance.</returns>
    public static CosmosDbVectorRecord Create<TItem>(
        string id,
        TItem item,
        string text,
        ReadOnlyMemory<float> embedding,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return CreateCore(id, item, text, embedding, options, null);
    }

    /// <summary>
    /// Creates a CosmosDbVectorRecord from an item using the specified type info.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="text">Text representation for embedding.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <param name="jsonTypeInfo">The JSON type info from a source-generated context.</param>
    /// <returns>A new CosmosDbVectorRecord instance.</returns>
    public static CosmosDbVectorRecord Create<TItem>(
        string id,
        TItem item,
        string text,
        ReadOnlyMemory<float> embedding,
        JsonTypeInfo<TItem> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        return CreateCore(id, item, text, embedding, null, jsonTypeInfo);
    }

    private static CosmosDbVectorRecord CreateCore<TItem>(
        string id,
        TItem item,
        string text,
        ReadOnlyMemory<float> embedding,
        JsonSerializerOptions? options,
        JsonTypeInfo<TItem>? jsonTypeInfo)
    {
        string itemJson;
        if (jsonTypeInfo is not null)
            itemJson = JsonSerializer.Serialize(item, jsonTypeInfo);
        else if (options is not null)
            itemJson = JsonSerializer.Serialize(item, options);
        else
            itemJson = JsonSerializer.Serialize(item);

        return new CosmosDbVectorRecord
        {
            Id = id,
            ItemJson = itemJson,
            Text = text,
            Embedding = embedding
        };
    }
}
