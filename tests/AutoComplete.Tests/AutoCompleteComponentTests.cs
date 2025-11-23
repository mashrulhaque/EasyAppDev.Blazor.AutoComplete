using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.Options;
using Microsoft.AspNetCore.Components;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for the AutoComplete component basic functionality.
/// </summary>
public class AutoCompleteComponentTests : TestContext
{
    private readonly List<Product> _testProducts = new()
    {
        new Product { Id = 1, Name = "Apple", Category = "Fruit" },
        new Product { Id = 2, Name = "Banana", Category = "Fruit" },
        new Product { Id = 3, Name = "Carrot", Category = "Vegetable" },
        new Product { Id = 4, Name = "Date", Category = "Fruit" },
        new Product { Id = 5, Name = "Eggplant", Category = "Vegetable" }
    };

    [Fact]
    public void AutoComplete_RendersWithDefaultParameters()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name));

        // Assert
        cut.Find("input.ebd-ac-input").Should().NotBeNull();
        cut.Find(".ebd-ac-container").Should().NotBeNull();
    }

    [Fact]
    public void AutoComplete_DisplaysPlaceholder()
    {
        // Arrange
        var placeholder = "Search products...";

        // Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Placeholder, placeholder));

        // Assert
        var input = cut.Find("input.ebd-ac-input");
        input.GetAttribute("placeholder").Should().Be(placeholder);
    }

    [Fact]
    public async Task AutoComplete_FiltersItemsOnInput()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)); // Disable debounce for testing

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("App"));
        cut.WaitForAssertion(() =>
        {
            // Assert
            var dropdown = cut.Find(".ebd-ac-dropdown");
            dropdown.Should().NotBeNull();
            var items = cut.FindAll(".ebd-ac-item");
            items.Count.Should().Be(1);
            items[0].TextContent.Should().Contain("Apple");
        });
    }

    [Fact]
    public async Task AutoComplete_SelectsItemOnClick()
    {
        // Arrange
        Product? selectedProduct = null;
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<Product?>(this, value => selectedProduct = value)));

        var input = cut.Find("input.ebd-ac-input");

        // Act - Search and select first item
        await cut.InvokeAsync(() => input.Input("Ban"));
        cut.WaitForAssertion(() =>
        {
            var item = cut.Find(".ebd-ac-item");
            item.Click();
        });

        // Assert
        selectedProduct.Should().NotBeNull();
        selectedProduct!.Name.Should().Be("Banana");
    }

    [Fact]
    public void AutoComplete_DisplaysClearButton_WhenAllowClearIsTrue()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.AllowClear, true));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var clearButton = cut.Find(".ebd-ac-clear-btn");
            clearButton.Should().NotBeNull();
        });
    }

    [Fact]
    public void AutoComplete_HidesClearButton_WhenAllowClearIsFalse()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.AllowClear, false));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.FindAll(".ebd-ac-clear-btn").Should().BeEmpty();
    }

    [Fact]
    public async Task AutoComplete_ClearsInput_WhenClearButtonClicked()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.AllowClear, true)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("Apple"));
        cut.WaitForAssertion(() =>
        {
            var clearButton = cut.Find(".ebd-ac-clear-btn");
            clearButton.Click();
        });

        // Assert
        input.GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void AutoComplete_DisplaysNoResults_WhenNoMatchingItems()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("xyz");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var noResults = cut.Find(".ebd-ac-no-results");
            noResults.Should().NotBeNull();
            noResults.TextContent.Should().Be("No results found");
        });
    }

    [Fact]
    public void AutoComplete_UsesCustomNoResultsTemplate()
    {
        // Arrange
        var customMessage = "Nothing found!";
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.NoResultsTemplate, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-no-results");
                builder.AddContent(2, customMessage);
                builder.CloseElement();
            }));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("xyz");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var noResults = cut.Find(".custom-no-results");
            noResults.TextContent.Should().Be(customMessage);
        });
    }

    [Fact]
    public void AutoComplete_RespectsMinSearchLength()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 3)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act - Input less than MinSearchLength
        input.Input("Ap");

        // Assert - Dropdown should not be visible
        cut.FindAll(".ebd-ac-dropdown").Should().BeEmpty();

        // Act - Input meets MinSearchLength
        input.Input("App");

        // Assert - Dropdown should be visible
        cut.WaitForAssertion(() =>
        {
            cut.Find(".ebd-ac-dropdown").Should().NotBeNull();
        });
    }

    [Fact]
    public void AutoComplete_RespectsMaxDisplayedItems()
    {
        // Arrange
        var manyProducts = Enumerable.Range(1, 20)
            .Select(i => new Product { Id = i, Name = $"Product {i}", Category = "Test" })
            .ToList();

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, manyProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MaxDisplayedItems, 5)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Product");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Count.Should().BeLessThanOrEqualTo(5);
        });
    }

    [Fact]
    public void AutoComplete_IsDisabled_WhenDisabledParameterIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Disabled, true));

        // Assert
        var input = cut.Find("input.ebd-ac-input");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void AutoComplete_UsesCustomItemTemplate()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-item");
                builder.AddContent(2, $"{item.Name} - {item.Category}");
                builder.CloseElement();
            }));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var customItem = cut.Find(".custom-item");
            customItem.TextContent.Should().Contain("Apple - Fruit");
        });
    }

    [Fact]
    public void AutoComplete_AppliesThemeClass()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Theme, Theme.Dark));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.ClassList.Should().Contain("ebd-ac-theme-dark");
    }

    [Fact]
    public void AutoComplete_AppliesRTLDirection()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.RightToLeft, true));

        // Assert
        var container = cut.Find(".ebd-ac-container");
        container.GetAttribute("dir").Should().Be("rtl");
    }

    [Fact]
    public void AutoComplete_HasCorrectAriaAttributes()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.AriaLabel, "Product search"));

        // Assert
        var input = cut.Find("input.ebd-ac-input");
        input.GetAttribute("role").Should().Be("combobox");
        input.GetAttribute("aria-label").Should().Be("Product search");
        input.GetAttribute("aria-autocomplete").Should().Be("list");
        input.GetAttribute("aria-haspopup").Should().Be("listbox");
    }

    [Fact]
    public void AutoComplete_DisplaysHeaderTemplate()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.HeaderTemplate, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-header");
                builder.AddContent(2, "Search Results:");
                builder.CloseElement();
            }));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("A");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var header = cut.Find(".custom-header");
            header.TextContent.Should().Be("Search Results:");
        });
    }

    [Fact]
    public void AutoComplete_DisplaysFooterTemplate()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.FooterTemplate, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-footer");
                builder.AddContent(2, "End of results");
                builder.CloseElement();
            }));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("A");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var footer = cut.Find(".custom-footer");
            footer.TextContent.Should().Be("End of results");
        });
    }
}

/// <summary>
/// Test model for Product
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
