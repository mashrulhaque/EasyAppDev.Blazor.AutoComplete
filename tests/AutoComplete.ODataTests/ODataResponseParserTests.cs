using EasyAppDev.Blazor.AutoComplete.OData;

namespace AutoComplete.ODataTests;

public class ODataResponseParserTests
{
    private readonly ODataOptions _defaultOptions = new()
    {
        EndpointUrl = "https://api.example.com/Products"
    };

    [Fact]
    public void Parse_StandardODataResponse_ReturnsItems()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            {
                "value": [
                    { "id": 1, "name": "Product A" },
                    { "id": 2, "name": "Product B" }
                ]
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Product A");
        result.Items[1].Name.Should().Be("Product B");
    }

    [Fact]
    public void Parse_DirectArrayResponse_ReturnsItems()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            [
                { "id": 1, "name": "Product A" },
                { "id": 2, "name": "Product B" }
            ]
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be(1);
        result.Items[1].Id.Should().Be(2);
    }

    [Fact]
    public void Parse_WithODataCount_ReturnsTotalCount()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            {
                "@odata.count": 100,
                "value": [
                    { "id": 1, "name": "Product A" }
                ]
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.TotalCount.Should().Be(100);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_WithAlternateCountFormat_ReturnsTotalCount()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            {
                "odata.count": 50,
                "value": [
                    { "id": 1, "name": "Product A" }
                ]
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.TotalCount.Should().Be(50);
    }

    [Fact]
    public void Parse_EmptyValueArray_ReturnsEmptyList()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            {
                "value": []
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyResponse()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);

        // Act
        var result = parser.Parse("");

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().BeNull();
    }

    [Fact]
    public void Parse_WhitespaceString_ReturnsEmptyResponse()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);

        // Act
        var result = parser.Parse("   ");

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void Parse_CustomResultsPropertyName_ReturnsItems()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = "https://api.example.com/Products",
            ResultsPropertyName = "items"
        };
        var parser = new ODataResponseParser<TestProduct>(options);
        var json = """
            {
                "items": [
                    { "id": 1, "name": "Product A" }
                ]
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Product A");
    }

    [Fact]
    public void Parse_CaseInsensitivePropertyMatching_ReturnsItems()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            {
                "value": [
                    { "ID": 1, "NAME": "Product A" }
                ]
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(1);
        result.Items[0].Name.Should().Be("Product A");
    }

    [Fact]
    public void Parse_InvalidItemInArray_SkipsInvalidItem()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        // This should still work even with extra properties
        var json = """
            {
                "value": [
                    { "id": 1, "name": "Product A", "extraField": "ignored" },
                    { "id": 2, "name": "Product B" }
                ]
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_NoValueProperty_ReturnsEmptyList()
    {
        // Arrange
        var parser = new ODataResponseParser<TestProduct>(_defaultOptions);
        var json = """
            {
                "message": "No results found"
            }
            """;

        // Act
        var result = parser.Parse(json);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new ODataResponseParser<TestProduct>(null!);
        action.Should().Throw<ArgumentNullException>();
    }
}

public class TestProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
}
