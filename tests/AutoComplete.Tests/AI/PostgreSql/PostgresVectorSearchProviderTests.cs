using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Npgsql;

namespace AutoComplete.Tests.AI.PostgreSql;

/// <summary>
/// Tests for PostgresVectorSearchProvider.
/// Note: These are unit tests using mocks. Integration tests require a real PostgreSQL instance.
/// </summary>
public class PostgresVectorSearchProviderTests
{
    [Fact]
    public void Constructor_ThrowsOnNullVectorStore()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var act = () => new PostgresVectorSearchProvider<TestProduct>(null!, options);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("vectorStore");
    }

    [Fact]
    public void Constructor_ThrowsOnNullOptions()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);

        // Act
        var act = () => new PostgresVectorSearchProvider<TestProduct>(vectorStore, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_ValidatesOptions_ThrowsOnEmptyConnectionString()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "",
            CollectionName = "test"
        };

        // Act
        var act = () => new PostgresVectorSearchProvider<TestProduct>(vectorStore, options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("ConnectionString");
    }

    [Fact]
    public void Constructor_ValidatesOptions_ThrowsOnEmptyCollectionName()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "Host=localhost",
            CollectionName = ""
        };

        // Act
        var act = () => new PostgresVectorSearchProvider<TestProduct>(vectorStore, options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("CollectionName");
    }

    [Fact]
    public void Constructor_ValidatesOptions_ThrowsOnZeroEmbeddingDimensions()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "Host=localhost",
            CollectionName = "test",
            EmbeddingDimensions = 0
        };

        // Act
        var act = () => new PostgresVectorSearchProvider<TestProduct>(vectorStore, options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("EmbeddingDimensions");
    }

    [Fact]
    public void Constructor_ValidatesOptions_ThrowsOnNegativeBatchSize()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var options = new PostgresVectorSearchOptions
        {
            ConnectionString = "Host=localhost",
            CollectionName = "test",
            IndexBatchSize = -1
        };

        // Act
        var act = () => new PostgresVectorSearchProvider<TestProduct>(vectorStore, options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("IndexBatchSize");
    }

    [Fact]
    public void Provider_ImplementsIVectorSearchProvider()
    {
        // Assert
        typeof(PostgresVectorSearchProvider<TestProduct>)
            .Should().Implement<IVectorSearchProvider<TestProduct>>();
    }

    [Fact]
    public async Task SearchAsync_ThrowsOnNullOptions()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var provider = new PostgresVectorSearchProvider<TestProduct>(vectorStore, CreateValidOptions());

        var embedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f });

        // Act
        var act = async () => await provider.SearchAsync(embedding, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(e => e.ParamName == "options");
    }

    [Fact]
    public async Task GetItemCountAsync_ReturnsNegativeOne()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var provider = new PostgresVectorSearchProvider<TestProduct>(vectorStore, CreateValidOptions());

        // Act
        var count = await provider.GetItemCountAsync();

        // Assert - Documented limitation: SK doesn't expose count
        count.Should().Be(-1);
    }

    private static PostgresVectorSearchOptions CreateValidOptions()
    {
        return new PostgresVectorSearchOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            CollectionName = "test_collection"
        };
    }

    private static NpgsqlDataSource CreateMockDataSource()
    {
        // Create a minimal data source - won't actually connect
        var builder = new NpgsqlDataSourceBuilder("Host=localhost;Database=test");
        builder.UseVector();
        return builder.Build();
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
