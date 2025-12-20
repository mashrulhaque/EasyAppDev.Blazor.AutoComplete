using Microsoft.Extensions.DependencyInjection;
using EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Extensions;
using EasyAppDev.Blazor.AutoComplete.AI.Qdrant.Models;

namespace AutoComplete.Tests.AI.Qdrant;

/// <summary>
/// Tests for Qdrant service extension methods.
/// </summary>
public class QdrantServiceExtensionsTests
{
    [Fact]
    public void AddAutoCompleteQdrantProvider_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAutoCompleteQdrantProvider<TestProduct>(opts =>
        {
            opts.Host = "localhost";
            opts.CollectionName = "test-collection";
        });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAutoCompleteQdrantProvider_ThrowsOnNullConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteQdrantProvider<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configureOptions");
    }

    [Fact]
    public void AddAutoCompleteQdrantProvider_ThrowsOnMissingCollectionName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteQdrantProvider<TestProduct>(opts =>
        {
            opts.Host = "localhost";
            // Missing CollectionName
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*CollectionName*");
    }

    [Fact]
    public void AddAutoCompleteQdrantProvider_ThrowsOnInvalidPort()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteQdrantProvider<TestProduct>(opts =>
        {
            opts.Host = "localhost";
            opts.CollectionName = "test";
            opts.Port = -1;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Port*");
    }

    [Fact]
    public void AddAutoCompleteQdrantProvider_ThrowsOnInvalidEmbeddingDimensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteQdrantProvider<TestProduct>(opts =>
        {
            opts.Host = "localhost";
            opts.CollectionName = "test";
            opts.EmbeddingDimensions = 0;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*EmbeddingDimensions*");
    }

    [Fact]
    public void AddAutoCompleteQdrantProvider_ThrowsOnInvalidBatchSize()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteQdrantProvider<TestProduct>(opts =>
        {
            opts.Host = "localhost";
            opts.CollectionName = "test";
            opts.IndexBatchSize = -1;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IndexBatchSize*");
    }

    [Fact]
    public void AddAutoCompleteQdrantProvider_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompleteQdrantProvider<TestProduct>(opts =>
        {
            opts.Host = "qdrant.example.com";
            opts.Port = 6333;
            opts.Https = true;
            opts.ApiKey = "test-api-key";
            opts.CollectionName = "products";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<QdrantVectorSearchOptions>();
        options.Should().NotBeNull();
        options!.Host.Should().Be("qdrant.example.com");
        options.Port.Should().Be(6333);
        options.Https.Should().BeTrue();
        options.ApiKey.Should().Be("test-api-key");
        options.CollectionName.Should().Be("products");
    }

    [Fact]
    public void AddAutoCompleteQdrantIndexer_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAutoCompleteQdrantIndexer<TestProduct>(p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAutoCompleteQdrantIndexer_ThrowsOnNullTextSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteQdrantIndexer<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("textSelector");
    }

    [Fact]
    public void AddAutoCompleteQdrant_RegistersBothProviderAndIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - just verify no exceptions (full test requires mocked dependencies)
        var act = () => services.AddAutoCompleteQdrant<TestProduct>(
            opts =>
            {
                opts.Host = "localhost";
                opts.CollectionName = "products";
            },
            p => p.Name,
            p => Guid.NewGuid());

        // Assert
        act.Should().NotThrow();
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
