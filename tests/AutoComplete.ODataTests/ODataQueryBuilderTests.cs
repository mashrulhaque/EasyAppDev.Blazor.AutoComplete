using EasyAppDev.Blazor.AutoComplete.OData;

namespace AutoComplete.ODataTests;

public class ODataQueryBuilderTests
{
    private const string BaseUrl = "https://api.example.com/Products";

    [Fact]
    public void BuildQuery_SingleField_StartsWith_GeneratesCorrectFilter()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.StartsWith,
            CaseInsensitive = true,
            Top = 10
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        query.Should().Contain("$filter=");
        decodedQuery.Should().Contain("startswith");
        decodedQuery.Should().Contain("tolower(Name)");
        decodedQuery.Should().Contain("'test'");
        query.Should().Contain("$top=10");
    }

    [Fact]
    public void BuildQuery_SingleField_Contains_V4_GeneratesCorrectFilter()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.Contains,
            Version = ODataVersion.V4,
            CaseInsensitive = true
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("search", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        decodedQuery.Should().Contain("contains(tolower(Name),'search')");
    }

    [Fact]
    public void BuildQuery_SingleField_Contains_V3_GeneratesSubstringOf()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.Contains,
            Version = ODataVersion.V3,
            CaseInsensitive = true
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("search", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        // OData v3 uses substringof(needle, haystack) - note the reversed argument order
        decodedQuery.Should().Contain("substringof('search',tolower(Name))");
    }

    [Fact]
    public void BuildQuery_MultiField_GeneratesOrClause()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.StartsWith,
            CaseInsensitive = true
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", new[] { "Name", "Description" });
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        decodedQuery.Should().Contain(" or ");
        decodedQuery.Should().Contain("startswith(tolower(Name),'test')");
        decodedQuery.Should().Contain("startswith(tolower(Description),'test')");
    }

    [Fact]
    public void BuildQuery_EscapesSingleQuotes()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.StartsWith,
            CaseInsensitive = true
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("O'Brien", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        // OData escapes single quotes by doubling them
        decodedQuery.Should().Contain("o''brien");
    }

    [Fact]
    public void BuildQuery_WithAdditionalFilter_CombinesWithAnd()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.StartsWith,
            AdditionalFilter = "IsActive eq true"
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        decodedQuery.Should().Contain(" and ");
        decodedQuery.Should().Contain("IsActive eq true");
    }

    [Fact]
    public void BuildQuery_WithSelect_IncludesSelectClause()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            Select = new[] { "Id", "Name", "Description" }
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");

        // Assert
        query.Should().Contain("$select=Id,Name,Description");
    }

    [Fact]
    public void BuildQuery_WithOrderBy_IncludesOrderByClause()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            OrderBy = "Name desc"
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        decodedQuery.Should().Contain("$orderby=");
        decodedQuery.Should().Contain("Name desc");
    }

    [Fact]
    public void BuildQuery_WithCount_IncludesCountClause()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            IncludeCount = true
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");

        // Assert
        query.Should().Contain("$count=true");
    }

    [Fact]
    public void BuildQuery_CaseSensitive_DoesNotUseTolower()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.StartsWith,
            CaseInsensitive = false
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("Test", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        decodedQuery.Should().NotContain("tolower");
        decodedQuery.Should().Contain("startswith(Name,'Test')");
    }

    [Fact]
    public void BuildQuery_EmptySearchText_ReturnsOnlyAdditionalFilter()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            AdditionalFilter = "IsActive eq true"
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        decodedQuery.Should().Contain("IsActive eq true");
        decodedQuery.Should().NotContain("startswith");
        decodedQuery.Should().NotContain("contains");
    }

    [Fact]
    public void BuildQuery_UrlWithExistingQueryString_UsesAmpersand()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = "https://api.example.com/Products?api-version=1.0"
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");

        // Assert
        query.Should().StartWith("https://api.example.com/Products?api-version=1.0&");
    }

    [Fact]
    public void EscapeODataString_EscapesSingleQuotes()
    {
        // Act
        var result = ODataQueryBuilder.EscapeODataString("O'Brien's");

        // Assert
        result.Should().Be("O''Brien''s");
    }

    [Fact]
    public void EscapeODataString_HandlesEmptyString()
    {
        // Act
        var result = ODataQueryBuilder.EscapeODataString("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void EscapeODataString_HandlesNull()
    {
        // Act
        var result = ODataQueryBuilder.EscapeODataString(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BuildQuery_FuzzyFallback_UsesContains()
    {
        // Arrange
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.FuzzyFallback,
            Version = ODataVersion.V4
        };
        var builder = new ODataQueryBuilder(options);

        // Act
        var query = builder.BuildQuery("test", "Name");
        var decodedQuery = Uri.UnescapeDataString(query);

        // Assert
        // FuzzyFallback uses Contains for the server query
        decodedQuery.Should().Contain("contains(");
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new ODataQueryBuilder(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuildQuery_EmptyFieldNames_ThrowsArgumentException()
    {
        // Arrange
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var builder = new ODataQueryBuilder(options);

        // Act & Assert
        var action = () => builder.BuildQuery("test", Array.Empty<string>());
        action.Should().Throw<ArgumentException>();
    }
}
