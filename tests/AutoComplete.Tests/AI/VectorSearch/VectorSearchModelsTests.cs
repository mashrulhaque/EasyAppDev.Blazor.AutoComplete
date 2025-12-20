using FluentAssertions;
using EasyAppDev.Blazor.AutoComplete.AI.Models;

namespace AutoComplete.Tests.AI.VectorSearch;

public class VectorSearchModelsTests
{
    public class VectorSearchResultTests
    {
        [Fact]
        public void Create_WithRequiredProperties_Succeeds()
        {
            // Arrange & Act
            var result = new VectorSearchResult<TestItem>
            {
                Item = new TestItem { Name = "Test" },
                Score = 0.95f
            };

            // Assert
            result.Item.Name.Should().Be("Test");
            result.Score.Should().Be(0.95f);
            result.Id.Should().BeNull();
        }

        [Fact]
        public void Create_WithOptionalId_Succeeds()
        {
            // Arrange & Act
            var result = new VectorSearchResult<TestItem>
            {
                Item = new TestItem { Name = "Test" },
                Score = 0.85f,
                Id = "item-123"
            };

            // Assert
            result.Id.Should().Be("item-123");
        }

        [Fact]
        public void WithExpression_CreatesNewInstance()
        {
            // Arrange
            var original = new VectorSearchResult<TestItem>
            {
                Item = new TestItem { Name = "Original" },
                Score = 0.9f
            };

            // Act
            var modified = original with { Score = 0.8f };

            // Assert
            original.Score.Should().Be(0.9f);
            modified.Score.Should().Be(0.8f);
            ReferenceEquals(original, modified).Should().BeFalse();
        }
    }

    public class VectorSearchOptionsTests
    {
        [Fact]
        public void Defaults_AreCorrect()
        {
            // Act
            var options = new VectorSearchOptions();

            // Assert
            options.MaxResults.Should().Be(20);
            options.MinScore.Should().BeNull();
            options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
            options.EnableHybridSearch.Should().BeFalse();
            options.TextQuery.Should().BeNull();
        }

        [Fact]
        public void Create_WithCustomValues_Succeeds()
        {
            // Act
            var options = new VectorSearchOptions
            {
                MaxResults = 50,
                MinScore = 0.7f,
                DistanceFunction = DistanceFunction.DotProduct,
                EnableHybridSearch = true,
                TextQuery = "search text"
            };

            // Assert
            options.MaxResults.Should().Be(50);
            options.MinScore.Should().Be(0.7f);
            options.DistanceFunction.Should().Be(DistanceFunction.DotProduct);
            options.EnableHybridSearch.Should().BeTrue();
            options.TextQuery.Should().Be("search text");
        }
    }

    public class VectorSearchDataSourceOptionsTests
    {
        [Fact]
        public void Defaults_AreCorrect()
        {
            // Act
            var options = new VectorSearchDataSourceOptions();

            // Assert
            options.MaxResults.Should().Be(20);
            options.MinSimilarityScore.Should().BeNull();
            options.DistanceFunction.Should().Be(DistanceFunction.Cosine);
            options.EnableHybridSearch.Should().BeFalse();
            options.QueryCacheDuration.Should().Be(TimeSpan.FromMinutes(15));
            options.MaxQueryCacheSize.Should().Be(1000);
        }

        [Fact]
        public void Create_WithCustomCacheSettings_Succeeds()
        {
            // Act
            var options = new VectorSearchDataSourceOptions
            {
                QueryCacheDuration = TimeSpan.FromMinutes(30),
                MaxQueryCacheSize = 5000
            };

            // Assert
            options.QueryCacheDuration.Should().Be(TimeSpan.FromMinutes(30));
            options.MaxQueryCacheSize.Should().Be(5000);
        }
    }

    public class DistanceFunctionTests
    {
        [Fact]
        public void AllValues_AreDefined()
        {
            // Act
            var values = Enum.GetValues<DistanceFunction>();

            // Assert
            values.Should().Contain(DistanceFunction.Cosine);
            values.Should().Contain(DistanceFunction.Euclidean);
            values.Should().Contain(DistanceFunction.DotProduct);
            values.Should().Contain(DistanceFunction.Manhattan);
            values.Should().Contain(DistanceFunction.Hamming);
            values.Should().Contain(DistanceFunction.Jaccard);
        }

        [Theory]
        [InlineData(DistanceFunction.Cosine, 0)]
        [InlineData(DistanceFunction.Euclidean, 1)]
        [InlineData(DistanceFunction.DotProduct, 2)]
        [InlineData(DistanceFunction.Manhattan, 3)]
        [InlineData(DistanceFunction.Hamming, 4)]
        [InlineData(DistanceFunction.Jaccard, 5)]
        public void Values_HaveExpectedUnderlyingValue(DistanceFunction function, int expectedValue)
        {
            // Assert
            ((int)function).Should().Be(expectedValue);
        }
    }

    public class IndexingProgressEventArgsTests
    {
        [Fact]
        public void ProgressPercentage_CalculatesCorrectly()
        {
            // Arrange
            var args = new IndexingProgressEventArgs
            {
                TotalItems = 100,
                ProcessedItems = 25
            };

            // Assert
            args.ProgressPercentage.Should().Be(25.0);
        }

        [Fact]
        public void ProgressPercentage_WithZeroTotal_ReturnsZero()
        {
            // Arrange
            var args = new IndexingProgressEventArgs
            {
                TotalItems = 0,
                ProcessedItems = 0
            };

            // Assert
            args.ProgressPercentage.Should().Be(0.0);
        }

        [Fact]
        public void IsComplete_WhenAllProcessed_ReturnsTrue()
        {
            // Arrange
            var args = new IndexingProgressEventArgs
            {
                TotalItems = 100,
                ProcessedItems = 100
            };

            // Assert
            args.IsComplete.Should().BeTrue();
        }

        [Fact]
        public void IsComplete_WhenNotAllProcessed_ReturnsFalse()
        {
            // Arrange
            var args = new IndexingProgressEventArgs
            {
                TotalItems = 100,
                ProcessedItems = 50
            };

            // Assert
            args.IsComplete.Should().BeFalse();
        }

        [Fact]
        public void AllProperties_CanBeSet()
        {
            // Act
            var args = new IndexingProgressEventArgs
            {
                TotalItems = 1000,
                ProcessedItems = 500,
                SuccessfulItems = 495,
                FailedItems = 5,
                Message = "Processing batch 5 of 10"
            };

            // Assert
            args.TotalItems.Should().Be(1000);
            args.ProcessedItems.Should().Be(500);
            args.SuccessfulItems.Should().Be(495);
            args.FailedItems.Should().Be(5);
            args.Message.Should().Be("Processing batch 5 of 10");
            args.ProgressPercentage.Should().Be(50.0);
        }
    }

    private class TestItem
    {
        public string Name { get; init; } = string.Empty;
    }
}
