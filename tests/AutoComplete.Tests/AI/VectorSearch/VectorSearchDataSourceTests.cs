using FluentAssertions;
using Moq;
using Microsoft.Extensions.AI;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.DataSources;
using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace AutoComplete.Tests.AI.VectorSearch;

public class VectorSearchDataSourceTests
{
    private readonly Mock<IVectorSearchProvider<Product>> _providerMock;
    private readonly Mock<IEmbeddingGenerator<string, Embedding<float>>> _embeddingMock;

    public VectorSearchDataSourceTests()
    {
        _providerMock = new Mock<IVectorSearchProvider<Product>>();
        _embeddingMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
    }

    public class Constructor : VectorSearchDataSourceTests
    {
        [Fact]
        public void WithNullProvider_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new VectorSearchDataSource<Product>(
                null!,
                _embeddingMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("provider");
        }

        [Fact]
        public void WithNullEmbeddingGenerator_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new VectorSearchDataSource<Product>(
                _providerMock.Object,
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("embeddingGenerator");
        }

        [Fact]
        public void WithValidParameters_CreatesInstance()
        {
            // Act
            var dataSource = new VectorSearchDataSource<Product>(
                _providerMock.Object,
                _embeddingMock.Object);

            // Assert
            dataSource.Should().NotBeNull();
        }

        [Fact]
        public void WithNullOptions_UsesDefaults()
        {
            // Act
            var dataSource = new VectorSearchDataSource<Product>(
                _providerMock.Object,
                _embeddingMock.Object,
                options: null);

            // Assert
            dataSource.Should().NotBeNull();
            dataSource.CachedQueryCount.Should().Be(0);
        }

        [Fact]
        public void WithCustomOptions_UsesProvidedOptions()
        {
            // Arrange
            var options = new VectorSearchDataSourceOptions
            {
                MaxResults = 50,
                MinSimilarityScore = 0.8f,
                DistanceFunction = DistanceFunction.DotProduct
            };

            // Act
            var dataSource = new VectorSearchDataSource<Product>(
                _providerMock.Object,
                _embeddingMock.Object,
                options);

            // Assert
            dataSource.Should().NotBeNull();
        }
    }

    public class SearchAsync : VectorSearchDataSourceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public async Task WithEmptyOrWhitespaceQuery_ReturnsEmptyResults(string? query)
        {
            // Arrange
            var dataSource = CreateDataSource();

            // Act
            var results = await dataSource.SearchAsync(query!);

            // Assert
            results.Should().BeEmpty();
            _embeddingMock.Verify(
                x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task WithValidQuery_GeneratesEmbeddingAndSearches()
        {
            // Arrange
            var dataSource = CreateDataSource();
            var searchText = "wireless headphones";
            var expectedProduct = new Product { Id = 1, Name = "Sony WH-1000XM5" };

            SetupMocksForSuccessfulSearch(searchText, expectedProduct);

            // Act
            var results = await dataSource.SearchAsync(searchText);

            // Assert
            results.Should().ContainSingle()
                .Which.Should().Be(expectedProduct);
        }

        [Fact]
        public async Task WithCachedQuery_DoesNotRegenerateEmbedding()
        {
            // Arrange
            var dataSource = CreateDataSource();
            var searchText = "test query";

            SetupMocksForSuccessfulSearch(searchText, new Product());

            // Act
            await dataSource.SearchAsync(searchText);
            await dataSource.SearchAsync(searchText);  // Same query again

            // Assert - embedding generated only once
            _embeddingMock.Verify(
                x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task WithCaseDifferentQuery_CachesNormalized()
        {
            // Arrange
            var dataSource = CreateDataSource();

            SetupMocksForSuccessfulSearch("test", new Product());

            // Act
            await dataSource.SearchAsync("TEST");
            await dataSource.SearchAsync("test");  // Same query, different case
            await dataSource.SearchAsync("  Test  ");  // Same query, with whitespace

            // Assert - embedding generated only once (all normalize to "test")
            _embeddingMock.Verify(
                x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PropagatesCancellationToken()
        {
            // Arrange
            var dataSource = CreateDataSource();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            SetupMocksForSuccessfulSearch("test", new Product());

            // Act
            await dataSource.SearchAsync("test", token);

            // Assert
            _providerMock.Verify(
                x => x.SearchAsync(
                    It.IsAny<ReadOnlyMemory<float>>(),
                    It.IsAny<VectorSearchOptions>(),
                    token),
                Times.Once);
        }

        [Fact]
        public async Task PassesCorrectSearchOptions()
        {
            // Arrange
            var options = new VectorSearchDataSourceOptions
            {
                MaxResults = 15,
                MinSimilarityScore = 0.6f,
                DistanceFunction = DistanceFunction.Euclidean,
                EnableHybridSearch = true
            };
            var dataSource = CreateDataSource(options);
            VectorSearchOptions? capturedOptions = null;

            SetupMocksForSuccessfulSearch("test", new Product());

            _providerMock
                .Setup(x => x.SearchAsync(
                    It.IsAny<ReadOnlyMemory<float>>(),
                    It.IsAny<VectorSearchOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ReadOnlyMemory<float>, VectorSearchOptions, CancellationToken>(
                    (_, opts, _) => capturedOptions = opts)
                .ReturnsAsync(Array.Empty<VectorSearchResult<Product>>());

            // Act
            await dataSource.SearchAsync("test");

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.MaxResults.Should().Be(15);
            capturedOptions.MinScore.Should().Be(0.6f);
            capturedOptions.DistanceFunction.Should().Be(DistanceFunction.Euclidean);
            capturedOptions.EnableHybridSearch.Should().BeTrue();
            capturedOptions.TextQuery.Should().Be("test");
        }

        [Fact]
        public async Task WithMultipleResults_ReturnsAllItems()
        {
            // Arrange
            var dataSource = CreateDataSource();
            var products = new[]
            {
                new Product { Id = 1, Name = "Product A" },
                new Product { Id = 2, Name = "Product B" },
                new Product { Id = 3, Name = "Product C" }
            };

            var embedding = CreateTestEmbedding();
            _embeddingMock
                .Setup(x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>(new[] { embedding }));

            _providerMock
                .Setup(x => x.SearchAsync(
                    It.IsAny<ReadOnlyMemory<float>>(),
                    It.IsAny<VectorSearchOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(products.Select((p, i) => new VectorSearchResult<Product>
                {
                    Item = p,
                    Score = 1.0f - (i * 0.1f)
                }));

            // Act
            var results = await dataSource.SearchAsync("test");

            // Assert
            results.Should().HaveCount(3);
            results.Should().ContainInOrder(products);
        }
    }

    public class CacheStatistics : VectorSearchDataSourceTests
    {
        [Fact]
        public async Task AfterCacheHit_UpdatesHitRate()
        {
            // Arrange
            var dataSource = CreateDataSource();
            SetupMocksForSuccessfulSearch("test", new Product());

            // Act
            await dataSource.SearchAsync("test");  // Miss
            await dataSource.SearchAsync("test");  // Hit
            await dataSource.SearchAsync("test");  // Hit

            // Assert
            dataSource.CacheHits.Should().Be(2);
            dataSource.CacheMisses.Should().Be(1);
            dataSource.QueryCacheHitRate.Should().BeApproximately(0.666, 0.01);
        }

        [Fact]
        public async Task AfterMultipleQueries_CountsCachedQueries()
        {
            // Arrange
            var dataSource = CreateDataSource();
            SetupMocksForSuccessfulSearch("query1", new Product());
            SetupMocksForSuccessfulSearch("query2", new Product());

            // Act
            await dataSource.SearchAsync("query1");
            await dataSource.SearchAsync("query2");

            // Assert
            dataSource.CachedQueryCount.Should().Be(2);
        }
    }

    public class CacheManagement : VectorSearchDataSourceTests
    {
        [Fact]
        public async Task ClearCacheAsync_RemovesAllCachedQueries()
        {
            // Arrange
            var dataSource = CreateDataSource();
            SetupMocksForSuccessfulSearch("test", new Product());

            await dataSource.SearchAsync("test");
            dataSource.CachedQueryCount.Should().Be(1);

            // Act
            await dataSource.ClearCacheAsync();

            // Assert
            dataSource.CachedQueryCount.Should().Be(0);
        }

        [Fact]
        public async Task CleanupCacheAsync_RemovesExpiredEntries()
        {
            // Arrange
            var options = new VectorSearchDataSourceOptions
            {
                QueryCacheDuration = TimeSpan.FromMilliseconds(50)
            };
            var dataSource = CreateDataSource(options);
            SetupMocksForSuccessfulSearch("test", new Product());

            await dataSource.SearchAsync("test");
            dataSource.CachedQueryCount.Should().Be(1);

            // Wait for entry to expire
            await Task.Delay(100);

            // Act
            await dataSource.CleanupCacheAsync();

            // Assert
            dataSource.CachedQueryCount.Should().Be(0);
        }
    }

    public class ProviderDelegation : VectorSearchDataSourceTests
    {
        [Fact]
        public async Task IsAvailableAsync_DelegatesToProvider()
        {
            // Arrange
            var dataSource = CreateDataSource();
            _providerMock.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await dataSource.IsAvailableAsync();

            // Assert
            result.Should().BeTrue();
            _providerMock.Verify(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetItemCountAsync_DelegatesToProvider()
        {
            // Arrange
            var dataSource = CreateDataSource();
            _providerMock.Setup(x => x.GetItemCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1000L);

            // Act
            var result = await dataSource.GetItemCountAsync();

            // Assert
            result.Should().Be(1000L);
            _providerMock.Verify(x => x.GetItemCountAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class Dispose : VectorSearchDataSourceTests
    {
        [Fact]
        public void CanBeCalledMultipleTimes()
        {
            // Arrange
            var dataSource = CreateDataSource();

            // Act
            var act = () =>
            {
                dataSource.Dispose();
                dataSource.Dispose();
            };

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public async Task AfterDispose_SearchAsyncThrowsObjectDisposedException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            dataSource.Dispose();

            // Act
            var act = () => dataSource.SearchAsync("test");

            // Assert
            await act.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task AfterDispose_IsAvailableAsyncThrowsObjectDisposedException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            dataSource.Dispose();

            // Act
            var act = () => dataSource.IsAvailableAsync();

            // Assert
            await act.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task AfterDispose_GetItemCountAsyncThrowsObjectDisposedException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            dataSource.Dispose();

            // Act
            var act = () => dataSource.GetItemCountAsync();

            // Assert
            await act.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task AfterDispose_ClearCacheAsyncThrowsObjectDisposedException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            dataSource.Dispose();

            // Act
            var act = () => dataSource.ClearCacheAsync();

            // Assert
            await act.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task AfterDispose_CleanupCacheAsyncThrowsObjectDisposedException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            dataSource.Dispose();

            // Act
            var act = () => dataSource.CleanupCacheAsync();

            // Assert
            await act.Should().ThrowAsync<ObjectDisposedException>();
        }
    }

    public class ErrorHandling : VectorSearchDataSourceTests
    {
        [Fact]
        public async Task WhenEmbeddingGeneratorFails_PropagatesException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            _embeddingMock
                .Setup(x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("API unavailable"));

            // Act
            var act = () => dataSource.SearchAsync("test");

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("API unavailable");
        }

        [Fact]
        public async Task WhenProviderFails_PropagatesException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            var embedding = CreateTestEmbedding();

            _embeddingMock
                .Setup(x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>(new[] { embedding }));

            _providerMock
                .Setup(x => x.SearchAsync(
                    It.IsAny<ReadOnlyMemory<float>>(),
                    It.IsAny<VectorSearchOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var act = () => dataSource.SearchAsync("test");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");
        }

        [Fact]
        public async Task WhenEmbeddingGeneratorReturnsEmpty_ThrowsInvalidOperationException()
        {
            // Arrange
            var dataSource = CreateDataSource();
            _embeddingMock
                .Setup(x => x.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>(Array.Empty<Embedding<float>>()));

            // Act
            var act = () => dataSource.SearchAsync("test");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to generate embedding for query");
        }
    }

    private VectorSearchDataSource<Product> CreateDataSource(
        VectorSearchDataSourceOptions? options = null)
    {
        return new VectorSearchDataSource<Product>(
            _providerMock.Object,
            _embeddingMock.Object,
            options);
    }

    private void SetupMocksForSuccessfulSearch(string query, Product expectedProduct)
    {
        var embedding = CreateTestEmbedding();

        _embeddingMock
            .Setup(x => x.GenerateAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>(new[] { embedding }));

        _providerMock
            .Setup(x => x.SearchAsync(
                It.IsAny<ReadOnlyMemory<float>>(),
                It.IsAny<VectorSearchOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new VectorSearchResult<Product>
                {
                    Item = expectedProduct,
                    Score = 0.9f
                }
            });
    }

    private static Embedding<float> CreateTestEmbedding(int dimensions = 1536)
    {
        var values = new float[dimensions];
        var random = new Random(42);
        for (int i = 0; i < dimensions; i++)
        {
            values[i] = (float)random.NextDouble();
        }
        return new Embedding<float>(values);
    }
}

public class Product
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
}
