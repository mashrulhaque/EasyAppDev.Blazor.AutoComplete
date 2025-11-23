using EasyAppDev.Blazor.AutoComplete;
using Microsoft.AspNetCore.Components;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for grouping functionality
/// </summary>
public class GroupingTests : TestContext
{
    private readonly List<Product> _testProducts = new()
    {
        new Product { Id = 1, Name = "Apple", Category = "Fruit" },
        new Product { Id = 2, Name = "Banana", Category = "Fruit" },
        new Product { Id = 3, Name = "Carrot", Category = "Vegetable" },
        new Product { Id = 4, Name = "Date", Category = "Fruit" },
        new Product { Id = 5, Name = "Eggplant", Category = "Vegetable" },
        new Product { Id = 6, Name = "Avocado", Category = "Fruit" }
    };

    [Fact]
    public void AutoComplete_DisplaysGroupedItems()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.GroupBy, p => p.Category)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("a");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var groups = cut.FindAll(".ebd-ac-group");
            groups.Should().NotBeEmpty();

            var groupHeaders = cut.FindAll(".ebd-ac-group-header");
            groupHeaders.Should().HaveCountGreaterThan(0);
        });
    }

    [Fact]
    public void AutoComplete_GroupsItemsByCategory()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.GroupBy, p => p.Category)
            .Add(p => p.FilterStrategy, EasyAppDev.Blazor.AutoComplete.Options.FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("a"); // Should match: Apple, Banana, Carrot, Date, Avocado

        // Assert
        cut.WaitForAssertion(() =>
        {
            var groupHeaders = cut.FindAll(".ebd-ac-group-header");
            groupHeaders.Should().HaveCount(2); // Fruit and Vegetable
        });
    }

    [Fact]
    public void AutoComplete_DisplaysGroupCount()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.GroupBy, p => p.Category)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("a");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var groupCounts = cut.FindAll(".ebd-ac-group-count");
            groupCounts.Should().NotBeEmpty();
            groupCounts.Any(gc => gc.TextContent.Contains("(")).Should().BeTrue();
        });
    }

    [Fact]
    public void AutoComplete_UsesCustomGroupTemplate()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.GroupBy, p => p.Category)
            .Add(p => p.GroupTemplate, group => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-group-header");
                builder.AddContent(2, $"Category: {group.Key}");
                builder.CloseElement();
            })
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("a");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var customHeaders = cut.FindAll(".custom-group-header");
            customHeaders.Should().NotBeEmpty();
            customHeaders.Any(h => h.TextContent.Contains("Category:")).Should().BeTrue();
        });
    }

    [Fact]
    public void AutoComplete_GroupedItemsAreSelectable()
    {
        // Arrange
        Product? selectedProduct = null;
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.GroupBy, p => p.Category)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<Product?>(this, value => selectedProduct = value)));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");
        cut.WaitForAssertion(() =>
        {
            var item = cut.Find(".ebd-ac-item");
            item.Click();
        });

        // Assert
        selectedProduct.Should().NotBeNull();
        selectedProduct!.Name.Should().Be("Apple");
    }

    [Fact]
    public void AutoComplete_WithoutGrouping_DoesNotDisplayGroups()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("a");

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-group").Should().BeEmpty();
            cut.FindAll(".ebd-ac-group-header").Should().BeEmpty();
        });
    }
}
