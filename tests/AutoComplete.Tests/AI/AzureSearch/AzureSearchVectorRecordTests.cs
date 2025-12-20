using System.Reflection;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

namespace AutoComplete.Tests.AI.AzureSearch;

/// <summary>
/// Tests for AzureSearchVectorRecord.
/// </summary>
public class AzureSearchVectorRecordTests
{
    [Fact]
    public void VectorRecord_HasRequiredProperties()
    {
        // Arrange
        var recordType = typeof(AzureSearchVectorRecord);

        // Assert - Check all expected properties exist
        recordType.GetProperty("Id").Should().NotBeNull();
        recordType.GetProperty("ItemJson").Should().NotBeNull();
        recordType.GetProperty("Content").Should().NotBeNull();
        recordType.GetProperty("Title").Should().NotBeNull();
        recordType.GetProperty("Embedding").Should().NotBeNull();
    }

    [Fact]
    public void VectorRecord_HasVectorStoreKeyAttribute_OnId()
    {
        // Arrange
        var idProperty = typeof(AzureSearchVectorRecord).GetProperty("Id");

        // Act
        var attributes = idProperty!.GetCustomAttributes().ToList();

        // Assert - Should have VectorStoreKey attribute
        attributes.Should().ContainSingle(a => a.GetType().Name == "VectorStoreKeyAttribute");
    }

    [Fact]
    public void VectorRecord_HasVectorStoreDataAttribute_OnDataProperties()
    {
        // Arrange
        var recordType = typeof(AzureSearchVectorRecord);

        // Assert
        HasVectorStoreDataAttribute(recordType.GetProperty("ItemJson")!).Should().BeTrue();
        HasVectorStoreDataAttribute(recordType.GetProperty("Content")!).Should().BeTrue();
        HasVectorStoreDataAttribute(recordType.GetProperty("Title")!).Should().BeTrue();
    }

    [Fact]
    public void VectorRecord_HasVectorStoreVectorAttribute_OnEmbedding()
    {
        // Arrange
        var embeddingProperty = typeof(AzureSearchVectorRecord).GetProperty("Embedding");

        // Act
        var attributes = embeddingProperty!.GetCustomAttributes().ToList();

        // Assert
        attributes.Should().ContainSingle(a => a.GetType().Name == "VectorStoreVectorAttribute");
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        // Arrange
        var testItem = new TestProduct { Id = 42, Name = "Test Product" };
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var record = AzureSearchVectorRecord.Create(
            id: "test-id",
            item: testItem,
            content: "Test Product description",
            title: "Test Product",
            embedding: embedding);

        // Assert
        record.Id.Should().Be("test-id");
        record.ItemJson.Should().Contain("Test Product");
        record.Content.Should().Be("Test Product description");
        record.Title.Should().Be("Test Product");
        record.Embedding.ToArray().Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public void Create_WithNullTitle_SetsNullTitle()
    {
        // Arrange
        var testItem = new TestProduct { Id = 1, Name = "Test" };
        var embedding = new float[] { 0.1f };

        // Act
        var record = AzureSearchVectorRecord.Create(
            id: "id",
            item: testItem,
            content: "content",
            title: null,
            embedding: embedding);

        // Assert
        record.Title.Should().BeNull();
    }

    [Fact]
    public void GetItem_DeserializesItemCorrectly()
    {
        // Arrange
        var originalItem = new TestProduct { Id = 123, Name = "Original Item" };
        var embedding = new float[] { 0.5f };
        var record = AzureSearchVectorRecord.Create(
            id: "test",
            item: originalItem,
            content: "Original Item",
            title: "Original Item",
            embedding: embedding);

        // Act
        var deserializedItem = record.GetItem<TestProduct>();

        // Assert
        deserializedItem.Should().NotBeNull();
        deserializedItem!.Id.Should().Be(123);
        deserializedItem.Name.Should().Be("Original Item");
    }

    [Fact]
    public void GetItem_WithEmptyJson_ReturnsDefault()
    {
        // Arrange - Create record with empty JSON (using reflection since ItemJson is required)
        var record = CreateRecordWithEmptyJson();

        // Act
        var result = record.GetItem<TestProduct>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Create_SerializesComplexObjects()
    {
        // Arrange
        var complexItem = new ComplexProduct
        {
            Id = 1,
            Name = "Complex",
            Tags = new List<string> { "tag1", "tag2" },
            Metadata = new Dictionary<string, string> { { "key", "value" } }
        };
        var embedding = new float[] { 0.1f };

        // Act
        var record = AzureSearchVectorRecord.Create(
            id: "complex",
            item: complexItem,
            content: "Complex product",
            title: "Complex",
            embedding: embedding);

        var deserialized = record.GetItem<ComplexProduct>();

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2" });
        deserialized.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void VectorRecord_IsPublic()
    {
        // Assert - Must be public for Azure Search SDK serialization
        typeof(AzureSearchVectorRecord).IsPublic.Should().BeTrue(
            "AzureSearchVectorRecord must be public for Azure Search SDK JSON serialization");
    }

    [Fact]
    public void VectorRecord_IsSealed()
    {
        // Assert
        typeof(AzureSearchVectorRecord).IsSealed.Should().BeTrue();
    }

    private static bool HasVectorStoreDataAttribute(PropertyInfo property)
    {
        return property.GetCustomAttributes()
            .Any(a => a.GetType().Name == "VectorStoreDataAttribute");
    }

    private static AzureSearchVectorRecord CreateRecordWithEmptyJson()
    {
        // Use Create method with a valid item, then we'll test with the actual empty scenario
        var record = AzureSearchVectorRecord.Create(
            id: "test",
            item: (object?)null,
            content: "test",
            title: null,
            embedding: new float[] { 0.1f });

        return record;
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class ComplexProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
