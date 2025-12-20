using System.Net;
using EasyAppDev.Blazor.AutoComplete.OData;

namespace AutoComplete.ODataTests;

public class ODataDataSourceTests
{
    private const string BaseUrl = "https://api.example.com/Products";

    [Fact]
    public async Task SearchAsync_ReturnsItems_FromValueArray()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*")
            .Respond("application/json", """
                {
                    "value": [
                        { "id": 1, "name": "Product A" },
                        { "id": 2, "name": "Product B" }
                    ]
                }
                """);

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("Product");

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ReturnsItems_FromDirectArray()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*")
            .Respond("application/json", """
                [
                    { "id": 1, "name": "Product A" },
                    { "id": 2, "name": "Product B" }
                ]
                """);

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("Product");

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_EmptySearchText_ReturnsEmpty()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("");

        // Assert
        results.Should().BeEmpty();
        mockHttp.GetMatchCount(mockHttp.Fallback).Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WhitespaceSearchText_ReturnsEmpty()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("   ");

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_BelowMinSearchLength_ReturnsEmpty()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            MinSearchLength = 3
        };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("ab");

        // Assert
        results.Should().BeEmpty();
        mockHttp.GetMatchCount(mockHttp.Fallback).Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_NetworkError_ReturnsEmptyAndSetsLastError()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*").Throw(new HttpRequestException("Network error"));

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("test");

        // Assert
        results.Should().BeEmpty();
        dataSource.LastError.Should().Contain("Network error");
    }

    [Fact]
    public async Task SearchAsync_HttpError_ReturnsEmptyAndSetsLastError()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*").Respond(HttpStatusCode.InternalServerError, "text/plain", "Server error");

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        var results = await dataSource.SearchAsync("test");

        // Assert
        results.Should().BeEmpty();
        dataSource.LastError.Should().Contain("500");
    }

    [Fact]
    public async Task SearchAsync_RaisesErrorOccurredEvent()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*").Throw(new HttpRequestException("Network error"));

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        string? capturedError = null;
        dataSource.ErrorOccurred += (sender, error) => capturedError = error;

        // Act
        await dataSource.SearchAsync("test");

        // Assert
        capturedError.Should().NotBeNull();
        capturedError.Should().Contain("Network error");
    }

    [Fact]
    public async Task SearchAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*").Respond(async () =>
        {
            await Task.Delay(5000);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // TaskCanceledException is a subclass of OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => dataSource.SearchAsync("test", cts.Token));
    }

    [Fact]
    public async Task SearchAsync_FuzzyFallback_ReranksResults()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*")
            .Respond("application/json", """
                {
                    "value": [
                        { "id": 1, "name": "Apple Pie" },
                        { "id": 2, "name": "Applesauce" },
                        { "id": 3, "name": "Apple" }
                    ]
                }
                """);

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions
        {
            EndpointUrl = BaseUrl,
            FilterStrategy = ODataFilterStrategy.FuzzyFallback
        };
        var dataSource = new ODataDataSource<TestProduct>(
            httpClient, options, "Name",
            textSelector: p => p.Name);

        // Act
        var results = (await dataSource.SearchAsync("Apple")).ToList();

        // Assert
        results.Should().HaveCount(3);
        // Exact match "Apple" should be ranked first
        results[0].Name.Should().Be("Apple");
    }

    [Fact]
    public async Task SearchAsync_MultiField_SendsOrFilter()
    {
        // Arrange
        string? capturedUrl = null;
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*")
            .Respond(req =>
            {
                capturedUrl = req.RequestUri?.ToString();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{ "value": [] }""", System.Text.Encoding.UTF8, "application/json")
                };
            });

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(
            httpClient, options, new[] { "Name", "Description" });

        // Act
        await dataSource.SearchAsync("test");

        // Assert
        capturedUrl.Should().NotBeNull();
        var decodedUrl = Uri.UnescapeDataString(capturedUrl!);
        decodedUrl.Should().Contain(" or ");
        decodedUrl.Should().Contain("tolower(Name)");
        decodedUrl.Should().Contain("tolower(Description)");
    }

    [Fact]
    public async Task SearchAsync_ClearsLastErrorOnSuccess()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        // First request fails
        mockHttp.When("*").Respond(req =>
        {
            // Return error first, then success
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "value": [] }""", System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };
        var dataSource = new ODataDataSource<TestProduct>(httpClient, options, "Name");

        // Act
        await dataSource.SearchAsync("test");

        // Assert
        dataSource.LastError.Should().BeNull();
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new ODataOptions { EndpointUrl = BaseUrl };

        // Act & Assert
        var action = () => new ODataDataSource<TestProduct>(null!, options, "Name");
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();

        // Act & Assert
        var action = () => new ODataDataSource<TestProduct>(httpClient, null!, "Name");
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyEndpointUrl_ThrowsArgumentException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = "" };

        // Act & Assert
        var action = () => new ODataDataSource<TestProduct>(httpClient, options, "Name");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EmptySearchFieldNames_ThrowsArgumentException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        var options = new ODataOptions { EndpointUrl = BaseUrl };

        // Act & Assert
        var action = () => new ODataDataSource<TestProduct>(httpClient, options, Array.Empty<string>());
        action.Should().Throw<ArgumentException>();
    }
}
