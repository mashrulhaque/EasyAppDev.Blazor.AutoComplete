using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

namespace AutoComplete.Tests.AI.AzureSearch;

/// <summary>
/// Tests for AzureSearchVectorSearchOptions.
/// </summary>
public class AzureSearchVectorSearchOptionsTests
{
    [Fact]
    public void Options_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new AzureSearchVectorSearchOptions();

        // Assert - verify all defaults
        options.Endpoint.Should().BeEmpty();
        options.ApiKey.Should().BeEmpty();
        options.IndexName.Should().BeEmpty();
        options.EmbeddingDimensions.Should().Be(1536, "default is OpenAI text-embedding-3-small dimension");
        options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
        options.EnableHybridSearch.Should().BeTrue("hybrid search is enabled by default");
        options.EnableSemanticRanking.Should().BeFalse("semantic ranking requires Standard tier");
        options.SemanticConfigurationName.Should().BeNull();
        options.VectorFieldName.Should().Be("Embedding");
        options.ContentFieldName.Should().Be("Content");
        options.IndexBatchSize.Should().Be(100);
    }

    [Fact]
    public void Options_CanSetProperties()
    {
        // Arrange & Act
        var options = new AzureSearchVectorSearchOptions
        {
            Endpoint = "https://my-search.search.windows.net",
            ApiKey = "my-api-key",
            IndexName = "products"
        };

        // Assert
        options.Endpoint.Should().Be("https://my-search.search.windows.net");
        options.ApiKey.Should().Be("my-api-key");
        options.IndexName.Should().Be("products");
    }

    [Fact]
    public void Options_CustomValues_AreApplied()
    {
        // Arrange & Act
        var options = new AzureSearchVectorSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            ApiKey = "test-key",
            IndexName = "test-index",
            EmbeddingDimensions = 384,
            DistanceFunction = DistanceFunction.Euclidean,
            EnableHybridSearch = false,
            EnableSemanticRanking = true,
            SemanticConfigurationName = "my-semantic-config",
            VectorFieldName = "CustomEmbedding",
            ContentFieldName = "CustomContent",
            IndexBatchSize = 500
        };

        // Assert
        options.EmbeddingDimensions.Should().Be(384);
        options.DistanceFunction.Should().Be(DistanceFunction.Euclidean);
        options.EnableHybridSearch.Should().BeFalse();
        options.EnableSemanticRanking.Should().BeTrue();
        options.SemanticConfigurationName.Should().Be("my-semantic-config");
        options.VectorFieldName.Should().Be("CustomEmbedding");
        options.ContentFieldName.Should().Be("CustomContent");
        options.IndexBatchSize.Should().Be(500);
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
        var options = new AzureSearchVectorSearchOptions
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
    public void Options_DistanceFunction_AcceptsAzureSearchSupportedValues(DistanceFunction function)
    {
        // Arrange & Act
        var options = new AzureSearchVectorSearchOptions
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
        var options = new AzureSearchVectorSearchOptions
        {
            Endpoint = "https://original.search.windows.net",
            IndexName = "original"
        };

        // Act
        options.IndexName = "modified";

        // Assert
        options.Endpoint.Should().Be("https://original.search.windows.net");
        options.IndexName.Should().Be("modified");
    }

    [Fact]
    public void Options_CanBeUsedWithConfigureDelegate()
    {
        // Arrange
        var options = new AzureSearchVectorSearchOptions();

        // Act - Simulate what service extensions do
        Action<AzureSearchVectorSearchOptions> configure = opts =>
        {
            opts.Endpoint = "https://test.search.windows.net";
            opts.ApiKey = "test-key";
            opts.IndexName = "test";
            opts.EmbeddingDimensions = 768;
            opts.EnableHybridSearch = true;
        };
        configure(options);

        // Assert
        options.Endpoint.Should().Be("https://test.search.windows.net");
        options.ApiKey.Should().Be("test-key");
        options.IndexName.Should().Be("test");
        options.EmbeddingDimensions.Should().Be(768);
        options.EnableHybridSearch.Should().BeTrue();
    }

    [Fact]
    public void Options_HybridAndSemanticSearch_CanBeEnabledTogether()
    {
        // Arrange & Act
        var options = new AzureSearchVectorSearchOptions
        {
            EnableHybridSearch = true,
            EnableSemanticRanking = true,
            SemanticConfigurationName = "my-config"
        };

        // Assert
        options.EnableHybridSearch.Should().BeTrue();
        options.EnableSemanticRanking.Should().BeTrue();
        options.SemanticConfigurationName.Should().Be("my-config");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void Options_IndexBatchSize_AcceptsValidValues(int batchSize)
    {
        // Arrange & Act
        var options = new AzureSearchVectorSearchOptions
        {
            IndexBatchSize = batchSize
        };

        // Assert
        options.IndexBatchSize.Should().Be(batchSize);
    }
}
