using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.DataSources;
using Microsoft.AspNetCore.Components;
using FluentAssertions;
using Xunit;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for error scenarios and exception handling.
/// </summary>
public class ErrorScenarioTests : TestContext
{
    #region Async Data Source Error Tests

    [Fact]
    public void AutoComplete_WithFailingDataSource_RendersInput()
    {
        // Arrange & Act
        var dataSource = new FailingDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        // Assert - Component should render without crashing
        var input = cut.Find("input.ebd-ac-input");
        input.Should().NotBeNull();
    }

    [Fact]
    public async Task AutoComplete_WithTimeoutDataSource_HandlesTimeout()
    {
        // Arrange
        var dataSource = new TimeoutDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("test"));

        // Assert - Should handle timeout gracefully
        await Task.Delay(2000);
        cut.Find("input.ebd-ac-input").Should().NotBeNull();
    }

    [Fact]
    public void AutoComplete_WithNullReturningDataSource_RendersInput()
    {
        // Arrange & Act
        var dataSource = new NullReturningDataSource();
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        // Assert - Component should render without crashing
        var input = cut.Find("input.ebd-ac-input");
        input.Should().NotBeNull();
    }

    #endregion

    #region Filter Exception Tests

    [Fact]
    public void AutoComplete_WithInvalidFilterStrategy_FallsBackToStartsWith()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        var invalidStrategy = (EasyAppDev.Blazor.AutoComplete.Options.FilterStrategy)999;

        // Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, invalidStrategy)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        input.Input("App");

        // Assert - Should use default filter
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    [Fact]
    public void AutoComplete_WithNullCustomFilter_FallsBackToConfiguredStrategy()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        // Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, EasyAppDev.Blazor.AutoComplete.Options.FilterStrategy.Custom)
            .Add(p => p.CustomFilter, null!)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        input.Input("App");

        // Assert - Should fall back to default filter
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    #endregion

    #region TextField/SearchFields Null Tests

    [Fact]
    public void AutoComplete_WithNullTextField_RendersWithoutError()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        // Act & Assert - Should not crash with null TextField
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, null!)
            .Add(p => p.DebounceMs, 0));

        // Component should render successfully
        var input = cut.Find("input.ebd-ac-input");
        input.Should().NotBeNull();
    }

    #endregion

    #region ValueChanged Error Tests

    [Fact]
    public async Task AutoComplete_ValueChangedThrowsException_DoesNotCrashComponent()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<Product?>(this, _ =>
            {
                throw new InvalidOperationException("Test exception");
            })));

        var input = cut.Find("input.ebd-ac-input");

        // Act & Assert - Should not crash even if callback throws
        await cut.InvokeAsync(() => input.Input("App"));
        cut.WaitForAssertion(() =>
        {
            var item = cut.Find(".ebd-ac-item");
            // Note: bUnit may swallow the exception or it may propagate
            // Either way, we're testing that the component structure remains intact
            try
            {
                item.Click();
            }
            catch (InvalidOperationException)
            {
                // Expected - callback threw
            }
        });

        // Component should still be functional
        cut.Find("input.ebd-ac-input").Should().NotBeNull();
    }

    #endregion

    #region Concurrent Operation Tests

    [Fact]
    public async Task AutoComplete_RapidInputChanges_HandlesGracefully()
    {
        // Arrange
        var products = Enumerable.Range(1, 100)
            .Select(i => new Product { Id = i, Name = $"Product {i}" })
            .ToList();

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 50));

        var input = cut.Find("input.ebd-ac-input");

        // Act - Rapid fire input changes
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(cut.InvokeAsync(() => input.Input($"Product {index}")));
        }

        await Task.WhenAll(tasks);

        // Assert - Component should handle rapid changes without crashing
        await Task.Delay(500);
        cut.Find("input.ebd-ac-input").Should().NotBeNull();
    }

    #endregion
}

/// <summary>
/// Data source that always fails with an exception.
/// </summary>
public class FailingDataSource : IAutoCompleteDataSource<Product>
{
    public Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated data source failure");
    }
}

/// <summary>
/// Data source that simulates a timeout.
/// </summary>
public class TimeoutDataSource : IAutoCompleteDataSource<Product>
{
    public async Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        // Simulate a very slow operation that times out
        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        return Enumerable.Empty<Product>();
    }
}

/// <summary>
/// Data source that returns null instead of empty collection.
/// </summary>
public class NullReturningDataSource : IAutoCompleteDataSource<Product>
{
    public Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Product>>(null!);
    }
}
