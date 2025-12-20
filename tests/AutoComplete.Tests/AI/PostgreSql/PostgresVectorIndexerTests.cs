using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.Models;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Npgsql;

namespace AutoComplete.Tests.AI.PostgreSql;

/// <summary>
/// Tests for PostgresVectorIndexer.
/// </summary>
public class PostgresVectorIndexerTests
{
    [Fact]
    public void Constructor_ThrowsOnNullVectorStore()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        // Act
        var act = () => new PostgresVectorIndexer<TestProduct>(
            null!,
            mockEmbeddingGenerator.Object,
            options,
            p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("vectorStore");
    }

    [Fact]
    public void Constructor_ThrowsOnNullEmbeddingGenerator()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var options = CreateValidOptions();

        // Act
        var act = () => new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            null!,
            options,
            p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("embeddingGenerator");
    }

    [Fact]
    public void Constructor_ThrowsOnNullOptions()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();

        // Act
        var act = () => new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            null!,
            p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_ThrowsOnNullTextSelector()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        // Act
        var act = () => new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            options,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("textSelector");
    }

    [Fact]
    public void Indexer_ImplementsIVectorIndexer()
    {
        // Assert
        typeof(PostgresVectorIndexer<TestProduct>)
            .Should().Implement<IVectorIndexer<TestProduct>>();
    }

    [Fact]
    public void Indexer_HasProgressChangedEvent()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        var indexer = new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            options,
            p => p.Name);

        // Assert - Event should be accessible
        var eventInfo = typeof(PostgresVectorIndexer<TestProduct>).GetEvent("ProgressChanged");
        eventInfo.Should().NotBeNull();
        eventInfo!.EventHandlerType.Should().Be(typeof(EventHandler<IndexingProgressEventArgs>));
    }

    [Fact]
    public async Task IndexAsync_WithEmptyCollection_ReturnsImmediately()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        var indexer = new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            options,
            p => p.Name);

        // Act - Should not throw with empty collection
        await indexer.IndexAsync(Array.Empty<TestProduct>());

        // Assert - Embedding generator should not be called
        mockEmbeddingGenerator.Verify(
            g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_ThrowsOnNullOrWhitespaceId()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        var indexer = new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            options,
            p => p.Name);

        // Act & Assert - null
        var actNull = async () => await indexer.RemoveAsync(null!);
        await actNull.Should().ThrowAsync<ArgumentException>();

        // Act & Assert - empty
        var actEmpty = async () => await indexer.RemoveAsync("");
        await actEmpty.Should().ThrowAsync<ArgumentException>();

        // Act & Assert - whitespace
        var actWhitespace = async () => await indexer.RemoveAsync("   ");
        await actWhitespace.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void Indexer_AcceptsOptionalIdSelector()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        // Act - Should not throw with custom ID selector
        var indexer = new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            options,
            textSelector: p => p.Name,
            idSelector: p => p.Id.ToString());

        // Assert
        indexer.Should().NotBeNull();
    }

    [Fact]
    public void Indexer_AcceptsNullIdSelector()
    {
        // Arrange
        using var dataSource = CreateMockDataSource();
        var vectorStore = new PostgresVectorStore(dataSource, ownsDataSource: false);
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var options = CreateValidOptions();

        // Act - Should not throw with null ID selector (uses GUID)
        var indexer = new PostgresVectorIndexer<TestProduct>(
            vectorStore,
            mockEmbeddingGenerator.Object,
            options,
            textSelector: p => p.Name,
            idSelector: null);

        // Assert
        indexer.Should().NotBeNull();
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
