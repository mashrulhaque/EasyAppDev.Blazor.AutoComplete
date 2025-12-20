using System.Reflection;
using Microsoft.Extensions.VectorData;

namespace AutoComplete.Tests.AI.CosmosDb;

/// <summary>
/// Tests for the internal CosmosDbVectorRecord class.
/// Uses reflection to access internal class for testing.
/// </summary>
public class CosmosDbVectorRecordTests
{
    private readonly Type _vectorRecordType;
    private readonly MethodInfo _createMethod;
    private readonly MethodInfo _getItemMethod;

    public CosmosDbVectorRecordTests()
    {
        // Get the internal CosmosDbVectorRecord type via reflection
        var assembly = typeof(EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.CosmosDbVectorSearchProvider<>).Assembly;
        _vectorRecordType = assembly.GetType("EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models.CosmosDbVectorRecord")!;
        _createMethod = _vectorRecordType.GetMethod("Create")!;
        _getItemMethod = _vectorRecordType.GetMethod("GetItem")!;
    }

    [Fact]
    public void CosmosDbVectorRecord_HasCorrectAttributes()
    {
        // Assert - VectorStoreKey on Id
        var idProperty = _vectorRecordType.GetProperty("Id");
        idProperty.Should().NotBeNull();
        idProperty!.GetCustomAttribute<VectorStoreKeyAttribute>().Should().NotBeNull();

        // Assert - VectorStoreData on ItemJson
        var itemJsonProperty = _vectorRecordType.GetProperty("ItemJson");
        itemJsonProperty.Should().NotBeNull();
        itemJsonProperty!.GetCustomAttribute<VectorStoreDataAttribute>().Should().NotBeNull();

        // Assert - VectorStoreData on Text
        var textProperty = _vectorRecordType.GetProperty("Text");
        textProperty.Should().NotBeNull();
        textProperty!.GetCustomAttribute<VectorStoreDataAttribute>().Should().NotBeNull();

        // Assert - VectorStoreVector on Embedding
        var embeddingProperty = _vectorRecordType.GetProperty("Embedding");
        embeddingProperty.Should().NotBeNull();
        embeddingProperty!.GetCustomAttribute<VectorStoreVectorAttribute>().Should().NotBeNull();
    }

    [Fact]
    public void CosmosDbVectorRecord_EmbeddingAttribute_HasCorrectDimensions()
    {
        // Arrange
        var embeddingProperty = _vectorRecordType.GetProperty("Embedding");
        var attribute = embeddingProperty!.GetCustomAttribute<VectorStoreVectorAttribute>();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Dimensions.Should().Be(1536, "default dimension for OpenAI embeddings");
    }

    [Fact]
    public void CosmosDbVectorRecord_Create_SerializesItemToJson()
    {
        // Arrange
        var testItem = new TestProduct { Id = 1, Name = "Test Product", Price = 9.99m };
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var genericMethod = _createMethod.MakeGenericMethod(typeof(TestProduct));
        var record = genericMethod.Invoke(null, new object[] { "item-1", testItem, "Test Product", new ReadOnlyMemory<float>(embedding) });

        // Assert
        var itemJsonProperty = _vectorRecordType.GetProperty("ItemJson");
        var itemJson = (string)itemJsonProperty!.GetValue(record)!;

        itemJson.Should().NotBeNullOrEmpty();
        itemJson.Should().Contain("Test Product");
        itemJson.Should().Contain("9.99");
    }

    [Fact]
    public void CosmosDbVectorRecord_GetItem_DeserializesJsonToItem()
    {
        // Arrange
        var testItem = new TestProduct { Id = 1, Name = "Test Product", Price = 9.99m };
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Create record
        var createGenericMethod = _createMethod.MakeGenericMethod(typeof(TestProduct));
        var record = createGenericMethod.Invoke(null, new object[] { "item-1", testItem, "Test Product", new ReadOnlyMemory<float>(embedding) });

        // Act
        var getItemGenericMethod = _getItemMethod.MakeGenericMethod(typeof(TestProduct));
        var result = (TestProduct?)getItemGenericMethod.Invoke(record, null);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(9.99m);
    }

    [Fact]
    public void CosmosDbVectorRecord_Create_SetsAllProperties()
    {
        // Arrange
        var testItem = new TestProduct { Id = 42, Name = "Widget", Price = 19.99m };
        var embedding = new float[] { 0.5f, 0.6f, 0.7f, 0.8f };
        var id = "unique-id-123";
        var text = "Widget description";

        // Act
        var genericMethod = _createMethod.MakeGenericMethod(typeof(TestProduct));
        var record = genericMethod.Invoke(null, new object[] { id, testItem, text, new ReadOnlyMemory<float>(embedding) });

        // Assert
        var idProperty = _vectorRecordType.GetProperty("Id");
        var textProperty = _vectorRecordType.GetProperty("Text");
        var embeddingProperty = _vectorRecordType.GetProperty("Embedding");

        idProperty!.GetValue(record).Should().Be(id);
        textProperty!.GetValue(record).Should().Be(text);

        var embeddingValue = (ReadOnlyMemory<float>)embeddingProperty!.GetValue(record)!;
        embeddingValue.ToArray().Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public void CosmosDbVectorRecord_IdProperty_IsString()
    {
        // Assert - CosmosDB uses string as key type
        var idProperty = _vectorRecordType.GetProperty("Id");
        idProperty.Should().NotBeNull();
        idProperty!.PropertyType.Should().Be(typeof(string));
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }
}
