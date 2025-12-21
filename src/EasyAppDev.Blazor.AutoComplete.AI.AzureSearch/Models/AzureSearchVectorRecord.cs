using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.VectorData;

namespace EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

/// <summary>
/// Record type for storing items with their embeddings in Azure AI Search.
/// The TItem is serialized as JSON for flexible storage.
/// Note: Must be public for Azure Search SDK JSON serialization to work.
/// </summary>
/// <remarks>
/// For AOT compatibility, use the overloads that accept <see cref="JsonSerializerOptions"/>
/// or <see cref="JsonTypeInfo{T}"/> with a source-generated context.
/// </remarks>
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
    /// <remarks>
    /// For AOT compatibility, configure options with a source-generated JsonSerializerContext:
    /// <code>
    /// var options = new JsonSerializerOptions { TypeInfoResolver = MyJsonContext.Default };
    /// var item = record.GetItem&lt;MyType&gt;(options);
    /// </code>
    /// </remarks>
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
    /// <remarks>
    /// This is the preferred method for AOT-compatible deserialization:
    /// <code>
    /// var item = record.GetItem(MyJsonContext.Default.MyType);
    /// </code>
    /// </remarks>
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
    /// Creates an AzureSearchVectorRecord from an item.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="content">Text content for full-text search.</param>
    /// <param name="title">Optional title for filtering.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <returns>A new AzureSearchVectorRecord instance.</returns>
    /// <remarks>
    /// This method uses reflection-based serialization and is NOT AOT-compatible.
    /// For AOT scenarios, use <see cref="Create{TItem}(string, TItem, string, string?, ReadOnlyMemory{float}, JsonSerializerOptions)"/>
    /// or <see cref="Create{TItem}(string, TItem, string, string?, ReadOnlyMemory{float}, JsonTypeInfo{TItem})"/> instead.
    /// </remarks>
    public static AzureSearchVectorRecord Create<TItem>(
        string id,
        TItem item,
        string content,
        string? title,
        ReadOnlyMemory<float> embedding)
    {
        return CreateCore(id, item, content, title, embedding, null, null);
    }

    /// <summary>
    /// Creates an AzureSearchVectorRecord from an item using the specified options.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="content">Text content for full-text search.</param>
    /// <param name="title">Optional title for filtering.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <param name="options">JSON serializer options with a configured TypeInfoResolver for AOT compatibility.</param>
    /// <returns>A new AzureSearchVectorRecord instance.</returns>
    public static AzureSearchVectorRecord Create<TItem>(
        string id,
        TItem item,
        string content,
        string? title,
        ReadOnlyMemory<float> embedding,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return CreateCore(id, item, content, title, embedding, options, null);
    }

    /// <summary>
    /// Creates an AzureSearchVectorRecord from an item using the specified type info.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="id">Unique identifier.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="content">Text content for full-text search.</param>
    /// <param name="title">Optional title for filtering.</param>
    /// <param name="embedding">The embedding vector.</param>
    /// <param name="jsonTypeInfo">The JSON type info from a source-generated context.</param>
    /// <returns>A new AzureSearchVectorRecord instance.</returns>
    /// <remarks>
    /// This is the preferred method for AOT-compatible serialization:
    /// <code>
    /// var record = AzureSearchVectorRecord.Create(id, item, content, title, embedding, MyJsonContext.Default.MyType);
    /// </code>
    /// </remarks>
    public static AzureSearchVectorRecord Create<TItem>(
        string id,
        TItem item,
        string content,
        string? title,
        ReadOnlyMemory<float> embedding,
        JsonTypeInfo<TItem> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
        return CreateCore(id, item, content, title, embedding, null, jsonTypeInfo);
    }

    private static AzureSearchVectorRecord CreateCore<TItem>(
        string id,
        TItem item,
        string content,
        string? title,
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

        return new AzureSearchVectorRecord
        {
            Id = id,
            ItemJson = itemJson,
            Content = content,
            Title = title,
            Embedding = embedding
        };
    }
}
