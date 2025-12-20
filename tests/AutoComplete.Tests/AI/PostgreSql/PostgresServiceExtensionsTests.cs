using Microsoft.Extensions.DependencyInjection;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Extensions;
using EasyAppDev.Blazor.AutoComplete.AI.PostgreSql.Models;

namespace AutoComplete.Tests.AI.PostgreSql;

/// <summary>
/// Tests for PostgresServiceExtensions DI registration.
/// </summary>
public class PostgresServiceExtensionsTests
{
    [Fact]
    public void AddAutoCompletePostgresProvider_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost;Database=test";
            options.CollectionName = "products";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<PostgresVectorSearchOptions>();

        options.Should().NotBeNull();
        options!.ConnectionString.Should().Be("Host=localhost;Database=test");
        options.CollectionName.Should().Be("products");
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_RegistersVectorSearchProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost;Database=test";
            options.CollectionName = "products";
        });

        // Assert - Check service is registered (can't resolve without actual Postgres)
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVectorSearchProvider<TestProduct>));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        var act = () => services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "test";
            options.CollectionName = "test";
        });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_ThrowsOnNullConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePostgresProvider<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("configureOptions");
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_ThrowsOnEmptyConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "";
            options.CollectionName = "test";
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("ConnectionString");
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_ThrowsOnEmptyCollectionName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost";
            options.CollectionName = "";
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("CollectionName");
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_ThrowsOnInvalidEmbeddingDimensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost";
            options.CollectionName = "test";
            options.EmbeddingDimensions = 0;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("EmbeddingDimensions");
    }

    [Fact]
    public void AddAutoCompletePostgresProvider_ThrowsOnInvalidBatchSize()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost";
            options.CollectionName = "test";
            options.IndexBatchSize = -1;
        });

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("IndexBatchSize");
    }

    [Fact]
    public void AddAutoCompletePostgresIndexer_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        var act = () => services.AddAutoCompletePostgresIndexer<TestProduct>(p => p.Name);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddAutoCompletePostgresIndexer_ThrowsOnNullTextSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAutoCompletePostgresIndexer<TestProduct>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("textSelector");
    }

    [Fact]
    public void AddAutoCompletePostgresIndexer_RegistersVectorIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add provider first (required dependency)
        services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost;Database=test";
            options.CollectionName = "products";
        });

        // Act
        services.AddAutoCompletePostgresIndexer<TestProduct>(
            textSelector: p => p.Name,
            idSelector: p => p.Id.ToString());

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IVectorIndexer<TestProduct>));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAutoCompletePostgres_RegistersBothProviderAndIndexer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoCompletePostgres<TestProduct>(
            configureOptions: options =>
            {
                options.ConnectionString = "Host=localhost;Database=test";
                options.CollectionName = "products";
            },
            textSelector: p => p.Name,
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
    public void AddAutoCompletePostgresProvider_ReturnsSameServices_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAutoCompletePostgresProvider<TestProduct>(options =>
        {
            options.ConnectionString = "Host=localhost";
            options.CollectionName = "test";
        });

        // Assert
        result.Should().BeSameAs(services);
    }

    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
