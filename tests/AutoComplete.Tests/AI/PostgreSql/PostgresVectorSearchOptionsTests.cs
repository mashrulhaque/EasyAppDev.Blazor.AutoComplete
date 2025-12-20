using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;

namespace AutoComplete.Tests.AI.PostgreSql;

/// <summary>
/// Tests for PostgresVectorSearchOptions.
/// </summary>
public class PostgresVectorSearchOptionsTests
{
    [Fact]
    public void Options_CanSetProperties()
    {
        // Arrange & Act
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            CollectionName = "test_collection"
        };

        // Assert
        options.ConnectionString.Should().Be("Host=localhost;Database=test");
        options.CollectionName.Should().Be("test_collection");
    }

    [Fact]
    public void Options_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new PostgresVectorSearchOptions();

        // Assert - verify all defaults
        options.ConnectionString.Should().BeEmpty();
        options.CollectionName.Should().BeEmpty();
        options.EmbeddingDimensions.Should().Be(1536, "default is OpenAI text-embedding-3-small dimension");
        options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
        options.Schema.Should().Be("public");
        options.IndexBatchSize.Should().Be(100);
        options.CreateHnswIndex.Should().BeTrue();
        options.HnswM.Should().Be(16);
        options.HnswEfConstruction.Should().Be(64);
    }

    [Fact]
    public void Options_CustomValues_AreApplied()
    {
        // Arrange & Act
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "Host=localhost",
            CollectionName = "products",
            EmbeddingDimensions = 384,
            DistanceFunction = DistanceFunction.Euclidean,
            Schema = "vectors",
            IndexBatchSize = 50,
            CreateHnswIndex = false,
            HnswM = 32,
            HnswEfConstruction = 128
        };

        // Assert
        options.EmbeddingDimensions.Should().Be(384);
        options.DistanceFunction.Should().Be(DistanceFunction.Euclidean);
        options.Schema.Should().Be("vectors");
        options.IndexBatchSize.Should().Be(50);
        options.CreateHnswIndex.Should().BeFalse();
        options.HnswM.Should().Be(32);
        options.HnswEfConstruction.Should().Be(128);
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
        var options = new PostgresVectorSearchOptions
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
    [InlineData(DistanceFunction.Manhattan)]
    public void Options_DistanceFunction_AcceptsAllValues(DistanceFunction function)
    {
        // Arrange & Act
        var options = new PostgresVectorSearchOptions
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
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "original",
            CollectionName = "original"
        };

        // Act
        options.CollectionName = "modified";

        // Assert
        options.ConnectionString.Should().Be("original");
        options.CollectionName.Should().Be("modified");
    }

    [Fact]
    public void Options_CanBeUsedWithConfigureDelegate()
    {
        // Arrange
        var options = new PostgresVectorSearchOptions();

        // Act - Simulate what service extensions do
        Action<PostgresVectorSearchOptions> configure = opts =>
        {
            opts.ConnectionString = "Host=localhost";
            opts.CollectionName = "test";
            opts.EmbeddingDimensions = 768;
        };
        configure(options);

        // Assert
        options.ConnectionString.Should().Be("Host=localhost");
        options.CollectionName.Should().Be("test");
        options.EmbeddingDimensions.Should().Be(768);
    }
}
