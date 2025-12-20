using FluentAssertions;
using Moq;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using EasyAppDev.Blazor.AutoComplete.AI.Abstractions;
using EasyAppDev.Blazor.AutoComplete.AI.DataSources;
using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace AutoComplete.Tests.AI.VectorSearch;

// Must be public for Moq to create proxies for generic interfaces
public class ExtensionTestProduct
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class VectorSearchServiceExtensionsTests
{
    public class AddAutoCompleteVectorSearch
    {
        [Fact]
        public void WithNullServices_ThrowsArgumentNullException()
        {
            // Act
            var act = () => AutoCompleteVectorSearchExtensions.AddAutoCompleteVectorSearch<ExtensionTestProduct>(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("services");
        }

        [Fact]
        public void WithoutConfigureOptions_RegistersWithDefaults()
        {
            // Arrange
            var services = new ServiceCollection();
            var providerMock = new Mock<IVectorSearchProvider<ExtensionTestProduct>>();
            var embeddingMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();

            services.AddSingleton(providerMock.Object);
            services.AddSingleton(embeddingMock.Object);

            // Act
            services.AddAutoCompleteVectorSearch<ExtensionTestProduct>();

            // Assert
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<VectorSearchDataSourceOptions>();
            options.MaxResults.Should().Be(20);
            options.MinSimilarityScore.Should().BeNull();
            options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
        }

        [Fact]
        public void WithConfigureOptions_AppliesConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var providerMock = new Mock<IVectorSearchProvider<ExtensionTestProduct>>();
            var embeddingMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();

            services.AddSingleton(providerMock.Object);
            services.AddSingleton(embeddingMock.Object);

            // Act
            services.AddAutoCompleteVectorSearch<ExtensionTestProduct>(options =>
            {
                options.MaxResults = 50;
                options.MinSimilarityScore = 0.7f;
                options.DistanceFunction = DistanceFunction.DotProduct;
            });

            // Assert - service can be resolved
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dataSource = scope.ServiceProvider.GetRequiredService<VectorSearchDataSource<ExtensionTestProduct>>();
            dataSource.Should().NotBeNull();
        }

        [Fact]
        public void RegistersVectorSearchDataSourceAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();
            var providerMock = new Mock<IVectorSearchProvider<ExtensionTestProduct>>();
            var embeddingMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();

            services.AddSingleton(providerMock.Object);
            services.AddSingleton(embeddingMock.Object);

            // Act
            services.AddAutoCompleteVectorSearch<ExtensionTestProduct>();

            // Assert
            var descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(VectorSearchDataSource<ExtensionTestProduct>));
            descriptor.Should().NotBeNull();
            descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void ReturnsServiceCollectionForChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAutoCompleteVectorSearch<ExtensionTestProduct>();

            // Assert
            result.Should().BeSameAs(services);
        }
    }

    public class AddAutoCompleteVectorSearchWithOpenAI
    {
        [Fact]
        public void WithNullServices_ThrowsArgumentNullException()
        {
            // Act
            var act = () => AutoCompleteVectorSearchExtensions.AddAutoCompleteVectorSearch<ExtensionTestProduct>(
                null!,
                openAiApiKey: "sk-test");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("services");
        }

        [Fact]
        public void WithNullApiKey_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearch<ExtensionTestProduct>(
                openAiApiKey: null!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("openAiApiKey");
        }

        [Fact]
        public void WithEmptyApiKey_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearch<ExtensionTestProduct>(
                openAiApiKey: "");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("openAiApiKey");
        }

        [Fact]
        public void WithWhitespaceApiKey_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearch<ExtensionTestProduct>(
                openAiApiKey: "   ");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("openAiApiKey");
        }
    }

    public class AddAutoCompleteVectorSearchWithAzure
    {
        [Fact]
        public void WithNullServices_ThrowsArgumentNullException()
        {
            // Act
            var act = () => AutoCompleteVectorSearchExtensions.AddAutoCompleteVectorSearchWithAzure<ExtensionTestProduct>(
                null!,
                endpoint: "https://test.azure.com",
                apiKey: "test-key",
                deploymentName: "embeddings");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("services");
        }

        [Fact]
        public void WithNullEndpoint_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearchWithAzure<ExtensionTestProduct>(
                endpoint: null!,
                apiKey: "test-key",
                deploymentName: "embeddings");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("endpoint");
        }

        [Fact]
        public void WithNullApiKey_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearchWithAzure<ExtensionTestProduct>(
                endpoint: "https://test.azure.com",
                apiKey: null!,
                deploymentName: "embeddings");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("apiKey");
        }

        [Fact]
        public void WithNullDeploymentName_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearchWithAzure<ExtensionTestProduct>(
                endpoint: "https://test.azure.com",
                apiKey: "test-key",
                deploymentName: null!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("deploymentName");
        }

        [Fact]
        public void WithEmptyEndpoint_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var act = () => services.AddAutoCompleteVectorSearchWithAzure<ExtensionTestProduct>(
                endpoint: "",
                apiKey: "test-key",
                deploymentName: "embeddings");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("endpoint");
        }
    }
}
