using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.DataSources;
using Microsoft.AspNetCore.Components;

namespace AutoComplete.Tests;

/// <summary>
/// Integration tests for async data sources
/// </summary>
public class AsyncDataSourceTests : TestContext
{
    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_LoadsData()
    {
        // Arrange
        var dataSource = new MockAsyncDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("test"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            dataSource.SearchWasCalled.Should().BeTrue();
            dataSource.LastSearchQuery.Should().Be("test");
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_DisplaysLoadingIndicator()
    {
        // Arrange
        var dataSource = new SlowMockDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("slow"));

        // Assert - Loading indicator should appear
        cut.WaitForAssertion(() =>
        {
            var loading = cut.Find(".ebd-ac-loading");
            loading.Should().NotBeNull();
        }, timeout: TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_DisplaysResults()
    {
        // Arrange
        var dataSource = new MockAsyncDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("Product"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().NotBeEmpty();
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_CancelsOngoingRequest()
    {
        // Arrange
        var dataSource = new SlowMockDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act - Trigger first search, then immediately trigger second
        await cut.InvokeAsync(() => input.Input("first"));
        await Task.Delay(50); // Short delay
        await cut.InvokeAsync(() => input.Input("second"));

        // Assert - First request should be cancelled
        await Task.Delay(1500); // Wait for both to potentially complete
        dataSource.CancellationWasRequested.Should().BeTrue();
    }

    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_UsesCustomLoadingTemplate()
    {
        // Arrange
        var dataSource = new SlowMockDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.LoadingTemplate, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-loading");
                builder.AddContent(2, "Please wait...");
                builder.CloseElement();
            }));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("test"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var customLoading = cut.Find(".custom-loading");
            customLoading.TextContent.Should().Be("Please wait...");
        }, timeout: TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_HandlesEmptyResults()
    {
        // Arrange
        var dataSource = new EmptyResultsDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("nothing"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var noResults = cut.Find(".ebd-ac-no-results");
            noResults.Should().NotBeNull();
        }, timeout: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task AutoComplete_WithAsyncDataSource_AllowsItemSelection()
    {
        // Arrange
        Product? selectedProduct = null;
        var dataSource = new MockAsyncDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<Product?>(this, value => selectedProduct = value)));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("Product"));

        cut.WaitForAssertion(() =>
        {
            var item = cut.Find(".ebd-ac-item");
            item.Click();
        }, timeout: TimeSpan.FromSeconds(3));

        // Assert
        selectedProduct.Should().NotBeNull();
    }
}

/// <summary>
/// Mock async data source for testing
/// </summary>
public class MockAsyncDataSource : IAutoCompleteDataSource<Product>
{
    public bool SearchWasCalled { get; private set; }
    public string? LastSearchQuery { get; private set; }

    private readonly List<Product> _data = new()
    {
        new Product { Id = 1, Name = "Product A", Category = "Category 1" },
        new Product { Id = 2, Name = "Product B", Category = "Category 1" },
        new Product { Id = 3, Name = "Product C", Category = "Category 2" }
    };

    public Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        SearchWasCalled = true;
        LastSearchQuery = query;

        // Simulate async operation
        return Task.FromResult(_data.Where(p =>
            p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
    }
}

/// <summary>
/// Slow mock data source to test loading states
/// </summary>
public class SlowMockDataSource : IAutoCompleteDataSource<Product>
{
    public bool CancellationWasRequested { get; private set; }

    private readonly List<Product> _data = new()
    {
        new Product { Id = 1, Name = "Slow Product A", Category = "Test" }
    };

    public async Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate slow network request
            await Task.Delay(1000, cancellationToken);
            return _data.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
        catch (OperationCanceledException)
        {
            CancellationWasRequested = true;
            throw;
        }
    }
}

/// <summary>
/// Data source that returns empty results
/// </summary>
public class EmptyResultsDataSource : IAutoCompleteDataSource<Product>
{
    public Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<Product>());
    }
}
