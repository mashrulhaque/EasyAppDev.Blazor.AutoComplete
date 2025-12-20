using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Models;

namespace AutoComplete.Tests.AI.Qdrant;

/// <summary>
/// Tests for QdrantVectorSearchOptions.
/// </summary>
public class QdrantVectorSearchOptionsTests
{
    [Fact]
    public void Options_CanSetProperties()
    {
        // Arrange & Act
        var options = new QdrantVectorSearchOptions
        {
            Host = "qdrant.example.com",
            CollectionName = "test-collection"
        };

        // Assert
        options.Host.Should().Be("qdrant.example.com");
        options.CollectionName.Should().Be("test-collection");
    }

    [Fact]
    public void Options_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new QdrantVectorSearchOptions();

        // Assert - verify all defaults
        options.Host.Should().Be("localhost");
        options.Port.Should().Be(6334);
        options.Https.Should().BeFalse();
        options.ApiKey.Should().BeNull();
        options.CollectionName.Should().BeEmpty();
        options.EmbeddingDimensions.Should().Be(1536, "default is OpenAI text-embedding-3-small dimension");
        options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
        options.IndexBatchSize.Should().Be(100);
        options.CreateHnswIndex.Should().BeTrue();
    }

    [Fact]
    public void Options_CustomValues_AreApplied()
    {
        // Arrange & Act
        var options = new QdrantVectorSearchOptions
        {
            Host = "cloud.qdrant.io",
            Port = 6333,
            Https = true,
            ApiKey = "qdrant-api-key",
            CollectionName = "products",
            EmbeddingDimensions = 384,
            DistanceFunction = DistanceFunction.Euclidean,
            IndexBatchSize = 50,
            CreateHnswIndex = false
        };

        // Assert
        options.Host.Should().Be("cloud.qdrant.io");
        options.Port.Should().Be(6333);
        options.Https.Should().BeTrue();
        options.ApiKey.Should().Be("qdrant-api-key");
        options.CollectionName.Should().Be("products");
        options.EmbeddingDimensions.Should().Be(384);
        options.DistanceFunction.Should().Be(DistanceFunction.Euclidean);
        options.IndexBatchSize.Should().Be(50);
        options.CreateHnswIndex.Should().BeFalse();
    }

    [Theory]
    [InlineData(256)]
    [InlineData(384)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1536)]
    [InlineData(3072)]
    public void Options_EmbeddingDimensions_AcceptsValidValues(int dimensions)
    {
        // Arrange & Act
        var options = new QdrantVectorSearchOptions
        {
            EmbeddingDimensions = dimensions
        };

        // Assert
        options.EmbeddingDimensions.Should().Be(dimensions);
    }

    [Theory]
    [InlineData(DistanceFunction.Cosine)]
    [InlineData(DistanceFunction.Euclidean)]
    [InlineData(DistanceFunction.DotProduct)]
    public void Options_DistanceFunction_AcceptsSupportedValues(DistanceFunction function)
    {
        // Arrange & Act
        var options = new QdrantVectorSearchOptions
        {
            DistanceFunction = function
        };

        // Assert
        options.DistanceFunction.Should().Be(function);
    }

    [Fact]
    public void Options_PropertiesCanBeModified()
    {
        // Arrange
        var options = new QdrantVectorSearchOptions
        {
            Host = "original",
            CollectionName = "original"
        };

        // Act
        options.CollectionName = "modified";

        // Assert
        options.Host.Should().Be("original");
        options.CollectionName.Should().Be("modified");
    }

    [Fact]
    public void Options_CanBeUsedWithConfigureDelegate()
    {
        // Arrange
        var options = new QdrantVectorSearchOptions();

        // Act - Simulate what service extensions do
        Action<QdrantVectorSearchOptions> configure = opts =>
        {
            opts.Host = "qdrant.local";
            opts.Port = 6334;
            opts.CollectionName = "configured-collection";
            opts.EmbeddingDimensions = 768;
        };
        configure(options);

        // Assert
        options.Host.Should().Be("qdrant.local");
        options.Port.Should().Be(6334);
        options.CollectionName.Should().Be("configured-collection");
        options.EmbeddingDimensions.Should().Be(768);
    }

    [Fact]
    public void Options_QdrantCloud_Configuration()
    {
        // Arrange & Act - typical Qdrant Cloud configuration
        var options = new QdrantVectorSearchOptions
        {
            Host = "xyz-12345.us-east4-0.gcp.cloud.qdrant.io",
            Port = 6333,
            Https = true,
            ApiKey = "your-qdrant-cloud-api-key",
            CollectionName = "products"
        };

        // Assert
        options.Host.Should().Contain("cloud.qdrant.io");
        options.Https.Should().BeTrue("Qdrant Cloud requires HTTPS");
        options.ApiKey.Should().NotBeNullOrEmpty("Qdrant Cloud requires API key");
    }

    [Fact]
    public void Options_SelfHosted_Configuration()
    {
        // Arrange & Act - typical self-hosted configuration
        var options = new QdrantVectorSearchOptions
        {
            Host = "localhost",
            Port = 6334,
            Https = false,
            CollectionName = "test-collection"
        };

        // Assert
        options.Host.Should().Be("localhost");
        options.Port.Should().Be(6334);
        options.Https.Should().BeFalse("local doesn't require HTTPS");
        options.ApiKey.Should().BeNull("self-hosted may not require API key");
    }
}
