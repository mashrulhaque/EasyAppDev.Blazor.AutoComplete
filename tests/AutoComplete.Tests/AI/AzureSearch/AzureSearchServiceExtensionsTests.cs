using Microsoft.Extensions.DependencyInjection;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Extensions;
using EasyAppDev.Blazor.AutoComplete.AI.AzureSearch.Models;

namespace AutoComplete.Tests.AI.AzureSearch;

/// <summary>
/// Tests for AzureSearchServiceExtensions DI registration.
/// </summary>
public class AzureSearchServiceExtensionsTests
{
    [Fact]
    public void AddAutoCompleteAzureSearchProvider_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "products";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<AzureSearchVectorSearchOptions>();

        options.Should().NotBeNull();
        options!.Endpoint.Should().Be("https://test.search.windows.net");
        options.ApiKey.Should().Be("test-key");
        options.IndexName.Should().Be("products");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_RegistersVectorSearchProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "products";
        });

        // Assert - Check service is registered (can't resolve without actual Azure Search)
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVectorSearchProvider<TestProduct>));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "test";
        });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnNullConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("configureOptions");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnEmptyEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "";
            options.ApiKey = "test-key";
            options.IndexName = "test";
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("Endpoint");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnEmptyApiKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "";
            options.IndexName = "test";
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("ApiKey");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnEmptyIndexName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "";
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("IndexName");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnInvalidEmbeddingDimensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "test";
            options.EmbeddingDimensions = 0;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("EmbeddingDimensions");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnInvalidBatchSize()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "test";
            options.IndexBatchSize = -1;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("IndexBatchSize");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ThrowsOnSemanticRankingWithoutConfig()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "test";
            options.EnableSemanticRanking = true;
            options.SemanticConfigurationName = null; // Missing required config
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("SemanticConfigurationName");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchIndexer_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        var act = () => services.AddAutoCompleteAzureSearchIndexer<TestProduct>(p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchIndexer_ThrowsOnNullTextSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompleteAzureSearchIndexer<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("textSelector");
    }

    [Fact]
    public void AddAutoCompleteAzureSearchIndexer_RegistersVectorIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add provider first (required dependency)
        services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "products";
        });

        // Act
        services.AddAutoCompleteAzureSearchIndexer<TestProduct>(
            textSelector: p => p.Name,
            titleSelector: p => p.Name,
            idSelector: p => p.Id.ToString());

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVectorIndexer<TestProduct>));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAutoCompleteAzureSearch_RegistersBothProviderAndIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompleteAzureSearch<TestProduct>(
            configureOptions: options =>
            {
                options.Endpoint = "https://test.search.windows.net";
                options.ApiKey = "test-key";
                options.IndexName = "products";
            },
            textSelector: p => p.Name,
            titleSelector: p => p.Name,
            idSelector: p => p.Id.ToString());

        // Assert
        var providerDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVectorSearchProvider<TestProduct>));
        var indexerDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVectorIndexer<TestProduct>));

        providerDescriptor.Should().NotBeNull();
        indexerDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_ReturnsSameServices_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "test";
        });

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAutoCompleteAzureSearchProvider_SemanticRankingWithConfig_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Should not throw when semantic config is provided
        var act = () => services.AddAutoCompleteAzureSearchProvider<TestProduct>(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.ApiKey = "test-key";
            options.IndexName = "test";
            options.EnableSemanticRanking = true;
            options.SemanticConfigurationName = "my-semantic-config";
        });

        // Assert
        act.Should().NotThrow();
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
