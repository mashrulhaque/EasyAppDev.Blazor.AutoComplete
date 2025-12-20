using Microsoft.Extensions.DependencyInjection;
using EasyAppDev.Blazor.AutoComplete.AI.Pinecone.Extensions;
using EasyAppDev.Blazor.AutoComplete.AI.Pinecone.Models;

namespace AutoComplete.Tests.AI.Pinecone;

/// <summary>
/// Tests for Pinecone service extension methods.
/// </summary>
public class PineconeServiceExtensionsTests
{
    [Fact]
    public void AddAutoCompletePineconeProvider_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAutoCompletePineconeProvider<TestProduct>(opts =>
        {
            opts.ApiKey = "test-key";
            opts.IndexName = "test-index";
        });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAutoCompletePineconeProvider_ThrowsOnNullConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePineconeProvider<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configureOptions");
    }

    [Fact]
    public void AddAutoCompletePineconeProvider_ThrowsOnMissingApiKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePineconeProvider<TestProduct>(opts =>
        {
            opts.IndexName = "test-index";
            // Missing ApiKey
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ApiKey*");
    }

    [Fact]
    public void AddAutoCompletePineconeProvider_ThrowsOnMissingIndexName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePineconeProvider<TestProduct>(opts =>
        {
            opts.ApiKey = "test-key";
            // Missing IndexName
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IndexName*");
    }

    [Fact]
    public void AddAutoCompletePineconeProvider_ThrowsOnInvalidEmbeddingDimensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePineconeProvider<TestProduct>(opts =>
        {
            opts.ApiKey = "test-key";
            opts.IndexName = "test-index";
            opts.EmbeddingDimensions = 0;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*EmbeddingDimensions*");
    }

    [Fact]
    public void AddAutoCompletePineconeProvider_ThrowsOnInvalidBatchSize()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePineconeProvider<TestProduct>(opts =>
        {
            opts.ApiKey = "test-key";
            opts.IndexName = "test-index";
            opts.IndexBatchSize = -1;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IndexBatchSize*");
    }

    [Fact]
    public void AddAutoCompletePineconeProvider_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompletePineconeProvider<TestProduct>(opts =>
        {
            opts.ApiKey = "pk-test-key";
            opts.IndexName = "products";
            opts.Namespace = "production";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<PineconeVectorSearchOptions>();
        options.Should().NotBeNull();
        options!.ApiKey.Should().Be("pk-test-key");
        options.IndexName.Should().Be("products");
        options.Namespace.Should().Be("production");
    }

    [Fact]
    public void AddAutoCompletePineconeIndexer_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAutoCompletePineconeIndexer<TestProduct>(p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAutoCompletePineconeIndexer_ThrowsOnNullTextSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePineconeIndexer<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("textSelector");
    }

    [Fact]
    public void AddAutoCompletePinecone_RegistersBothProviderAndIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - just verify no exceptions (full test requires mocked dependencies)
        var act = () => services.AddAutoCompletePinecone<TestProduct>(
            opts =>
            {
                opts.ApiKey = "pk-test-key";
                opts.IndexName = "products";
            },
            p => p.Name,
            p => p.Id.ToString());

        // Assert
        act.Should().NotThrow();
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
