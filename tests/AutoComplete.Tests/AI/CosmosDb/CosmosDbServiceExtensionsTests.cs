using Microsoft.Extensions.DependencyInjection;
using EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Extensions;
using EasyAppDev.Blazor.AutoComplete.AI.CosmosDb.Models;

namespace AutoComplete.Tests.AI.CosmosDb;

/// <summary>
/// Tests for CosmosDB service extension methods.
/// </summary>
public class CosmosDbServiceExtensionsTests
{
    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=key";
            opts.DatabaseName = "test-db";
            opts.ContainerName = "test-container";
        });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnNullConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbProvider<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configureOptions");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnMissingConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.DatabaseName = "test-db";
            opts.ContainerName = "test-container";
            // Missing ConnectionString
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ConnectionString*");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnMissingDatabaseName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=key";
            opts.ContainerName = "test-container";
            // Missing DatabaseName
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DatabaseName*");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnMissingContainerName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=key";
            opts.DatabaseName = "test-db";
            // Missing ContainerName
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ContainerName*");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnInvalidEmbeddingDimensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=key";
            opts.DatabaseName = "test-db";
            opts.ContainerName = "test-container";
            opts.EmbeddingDimensions = 0;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*EmbeddingDimensions*");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_ThrowsOnInvalidBatchSize()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=key";
            opts.DatabaseName = "test-db";
            opts.ContainerName = "test-container";
            opts.IndexBatchSize = -1;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IndexBatchSize*");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbProvider_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompleteCosmosDbProvider<TestProduct>(opts =>
        {
            opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=test-key";
            opts.DatabaseName = "ecommerce";
            opts.ContainerName = "products";
            opts.VectorIndexType = "diskANN";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<CosmosDbVectorSearchOptions>();
        options.Should().NotBeNull();
        options!.ConnectionString.Should().Contain("documents.azure.com");
        options.DatabaseName.Should().Be("ecommerce");
        options.ContainerName.Should().Be("products");
        options.VectorIndexType.Should().Be("diskANN");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbIndexer_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAutoCompleteCosmosDbIndexer<TestProduct>(p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAutoCompleteCosmosDbIndexer_ThrowsOnNullTextSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteCosmosDbIndexer<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("textSelector");
    }

    [Fact]
    public void AddAutoCompleteCosmosDb_RegistersBothProviderAndIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - just verify no exceptions (full test requires mocked dependencies)
        var act = () => services.AddAutoCompleteCosmosDb<TestProduct>(
            opts =>
            {
                opts.ConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=key";
                opts.DatabaseName = "test-db";
                opts.ContainerName = "products";
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
