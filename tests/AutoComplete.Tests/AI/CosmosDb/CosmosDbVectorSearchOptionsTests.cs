using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

namespace AutoComplete.Tests.AI.CosmosDb;

/// <summary>
/// Tests for CosmosDbVectorSearchOptions.
/// </summary>
public class CosmosDbVectorSearchOptionsTests
{
    [Fact]
    public void Options_CanSetProperties()
    {
        // Arrange & Act
        var options = new CosmosDbVectorSearchOptions
        {
            ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=test",
            DatabaseName = "test-db",
            ContainerName = "test-container"
        };

        // Assert
        options.ConnectionString.Should().Contain("documents.azure.com");
        options.DatabaseName.Should().Be("test-db");
        options.ContainerName.Should().Be("test-container");
    }

    [Fact]
    public void Options_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new CosmosDbVectorSearchOptions();

        // Assert - verify all defaults
        options.ConnectionString.Should().BeEmpty();
        options.DatabaseName.Should().BeEmpty();
        options.ContainerName.Should().BeEmpty();
        options.EmbeddingDimensions.Should().Be(1536, "default is OpenAI text-embedding-3-small dimension");
        options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
        options.IndexBatchSize.Should().Be(100);
        options.PartitionKeyPath.Should().Be("/id");
        options.VectorIndexType.Should().Be("quantizedFlat");
    }

    [Fact]
    public void Options_CustomValues_AreApplied()
    {
        // Arrange & Act
        var options = new CosmosDbVectorSearchOptions
        {
            ConnectionString = "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=key",
            DatabaseName = "ecommerce",
            ContainerName = "products",
            EmbeddingDimensions = 384,
            DistanceFunction = DistanceFunction.Euclidean,
            IndexBatchSize = 50,
            PartitionKeyPath = "/category",
            VectorIndexType = "diskANN"
        };

        // Assert
        options.ConnectionString.Should().Contain("myaccount");
        options.DatabaseName.Should().Be("ecommerce");
        options.ContainerName.Should().Be("products");
        options.EmbeddingDimensions.Should().Be(384);
        options.DistanceFunction.Should().Be(DistanceFunction.Euclidean);
        options.IndexBatchSize.Should().Be(50);
        options.PartitionKeyPath.Should().Be("/category");
        options.VectorIndexType.Should().Be("diskANN");
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
        var options = new CosmosDbVectorSearchOptions
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
        var options = new CosmosDbVectorSearchOptions
        {
            DistanceFunction = function
        };

        // Assert
        options.DistanceFunction.Should().Be(function);
    }

    [Theory]
    [InlineData("flat")]
    [InlineData("quantizedFlat")]
    [InlineData("diskANN")]
    public void Options_VectorIndexType_AcceptsValidValues(string indexType)
    {
        // Arrange & Act
        var options = new CosmosDbVectorSearchOptions
        {
            VectorIndexType = indexType
        };

        // Assert
        options.VectorIndexType.Should().Be(indexType);
    }

    [Fact]
    public void Options_PropertiesCanBeModified()
    {
        // Arrange
        var options = new CosmosDbVectorSearchOptions
        {
            ConnectionString = "original",
            ContainerName = "original"
        };

        // Act
        options.ContainerName = "modified";

        // Assert
        options.ConnectionString.Should().Be("original");
        options.ContainerName.Should().Be("modified");
    }

    [Fact]
    public void Options_CanBeUsedWithConfigureDelegate()
    {
        // Arrange
        var options = new CosmosDbVectorSearchOptions();

        // Act - Simulate what service extensions do
        Action<CosmosDbVectorSearchOptions> configure = opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://configured.documents.azure.com:443/;AccountKey=key";
            opts.DatabaseName = "configured-db";
            opts.ContainerName = "configured-container";
            opts.EmbeddingDimensions = 768;
            opts.VectorIndexType = "diskANN";
        };
        configure(options);

        // Assert
        options.ConnectionString.Should().Contain("configured");
        options.DatabaseName.Should().Be("configured-db");
        options.ContainerName.Should().Be("configured-container");
        options.EmbeddingDimensions.Should().Be(768);
        options.VectorIndexType.Should().Be("diskANN");
    }

    [Theory]
    [InlineData("/id")]
    [InlineData("/category")]
    [InlineData("/tenantId")]
    [InlineData("/userId")]
    public void Options_PartitionKeyPath_AcceptsValidPaths(string path)
    {
        // Arrange & Act
        var options = new CosmosDbVectorSearchOptions
        {
            PartitionKeyPath = path
        };

        // Assert
        options.PartitionKeyPath.Should().Be(path);
    }
}
