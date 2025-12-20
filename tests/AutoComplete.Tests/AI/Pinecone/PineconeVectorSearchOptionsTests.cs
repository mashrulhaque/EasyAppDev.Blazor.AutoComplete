using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.Pinecone.Models;

namespace AutoComplete.Tests.AI.Pinecone;

/// <summary>
/// Tests for PineconeVectorSearchOptions.
/// </summary>
public class PineconeVectorSearchOptionsTests
{
    [Fact]
    public void Options_CanSetProperties()
    {
        // Arrange & Act
        var options = new PineconeVectorSearchOptions
        {
            ApiKey = "pk-test-api-key",
            IndexName = "test-index"
        };

        // Assert
        options.ApiKey.Should().Be("pk-test-api-key");
        options.IndexName.Should().Be("test-index");
    }

    [Fact]
    public void Options_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new PineconeVectorSearchOptions();

        // Assert - verify all defaults
        options.ApiKey.Should().BeEmpty();
        options.IndexName.Should().BeEmpty();
        options.Namespace.Should().BeNull();
        options.EmbeddingDimensions.Should().Be(1536, "default is OpenAI text-embedding-3-small dimension");
        options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
        options.IndexBatchSize.Should().Be(100);
    }

    [Fact]
    public void Options_CustomValues_AreApplied()
    {
        // Arrange & Act
        var options = new PineconeVectorSearchOptions
        {
            ApiKey = "pk-my-api-key",
            IndexName = "products",
            Namespace = "production",
            EmbeddingDimensions = 384,
            DistanceFunction = DistanceFunction.Euclidean,
            IndexBatchSize = 50
        };

        // Assert
        options.ApiKey.Should().Be("pk-my-api-key");
        options.IndexName.Should().Be("products");
        options.Namespace.Should().Be("production");
        options.EmbeddingDimensions.Should().Be(384);
        options.DistanceFunction.Should().Be(DistanceFunction.Euclidean);
        options.IndexBatchSize.Should().Be(50);
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
        var options = new PineconeVectorSearchOptions
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
        var options = new PineconeVectorSearchOptions
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
        var options = new PineconeVectorSearchOptions
        {
            ApiKey = "original",
            IndexName = "original"
        };

        // Act
        options.IndexName = "modified";

        // Assert
        options.ApiKey.Should().Be("original");
        options.IndexName.Should().Be("modified");
    }

    [Fact]
    public void Options_CanBeUsedWithConfigureDelegate()
    {
        // Arrange
        var options = new PineconeVectorSearchOptions();

        // Act - Simulate what service extensions do
        Action<PineconeVectorSearchOptions> configure = opts =>
        {
            opts.ApiKey = "pk-configured-key";
            opts.IndexName = "configured-index";
            opts.Namespace = "test-namespace";
            opts.EmbeddingDimensions = 768;
        };
        configure(options);

        // Assert
        options.ApiKey.Should().Be("pk-configured-key");
        options.IndexName.Should().Be("configured-index");
        options.Namespace.Should().Be("test-namespace");
        options.EmbeddingDimensions.Should().Be(768);
    }
}
