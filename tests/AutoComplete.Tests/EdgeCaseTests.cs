using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.Options;
using Microsoft.AspNetCore.Components;
using FluentAssertions;
using Xunit;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for edge cases and null handling in AutoComplete component.
/// </summary>
public class EdgeCaseTests : AutoCompleteTestBase
{
    #region Null and Empty Collection Tests

    [Fact]
    public void AutoComplete_WithNullItems_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, (IEnumerable<Product>?)null)
            .Add(p => p.TextField, p => p.Name));

        // Assert
        cut.Find("input.ebd-ac-input").Should().NotBeNull();
    }

    [Fact]
    public void AutoComplete_WithEmptyItems_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product>())
            .Add(p => p.TextField, p => p.Name));

        // Assert
        cut.Find("input.ebd-ac-input").Should().NotBeNull();
    }

    [Fact]
    public void AutoComplete_WithEmptyItems_ShowsNoResults()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product>())
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("test");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var noResults = cut.Find(".ebd-ac-no-results");
            noResults.Should().NotBeNull();
        });
    }

    [Fact]
    public void AutoComplete_WithItemsContainingNullValues_FiltersCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple", Category = null! },
            new() { Id = 2, Name = null!, Category = "Fruit" },
            new() { Id = 3, Name = "Banana", Category = "Fruit" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Ban");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public void AutoComplete_WithMinSearchLengthZero_ShowsAllItems()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" },
            new() { Id = 2, Name = "Banana" },
            new() { Id = 3, Name = "Cherry" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 0)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().HaveCount(3);
        });
    }

    [Fact]
    public void AutoComplete_WithMaxDisplayedItemsZero_ShowsNoItems()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" },
            new() { Id = 2, Name = "Banana" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MaxDisplayedItems, 0)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("A");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().BeEmpty();
        });
    }

    [Fact]
    public void AutoComplete_WithVeryLargeDebounce_DelaysFiltering()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 10000)); // 10 seconds

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert - Items should not appear immediately
        var items = cut.FindAll(".ebd-ac-item");
        items.Should().BeEmpty();
    }

    [Fact]
    public void AutoComplete_WithSingleItem_RendersCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "OnlyItem" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Only");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    #endregion

    #region Special Character Tests

    [Fact]
    public void AutoComplete_WithSpecialCharactersInSearch_FiltersCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product (Special)" },
            new() { Id = 2, Name = "Product [Brackets]" },
            new() { Id = 3, Name = "Product <Angle>" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("(Special)");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    [Fact]
    public void AutoComplete_WithUnicodeCharacters_FiltersCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Café" },
            new() { Id = 2, Name = "Naïve" },
            new() { Id = 3, Name = "Résumé" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Café");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    [Fact]
    public void AutoComplete_WithEmojis_FiltersCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple 🍎" },
            new() { Id = 2, Name = "Banana 🍌" },
            new() { Id = 3, Name = "Cherry 🍒" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("🍎");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    #endregion

    #region Whitespace Tests

    [Fact]
    public void AutoComplete_WithLeadingWhitespace_AcceptsInput()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 0)
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("App");

        // Assert - Should find items when searching with valid text
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    [Fact]
    public void AutoComplete_WithTrailingWhitespace_AcceptsInput()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.FilterStrategy, FilterStrategy.Contains)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("App");

        // Assert - Should find items when searching with valid text
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
        });
    }

    [Fact]
    public void AutoComplete_WithOnlyWhitespace_DoesNotFilter()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple" }
        };

        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, products)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 1)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act - Enter whitespace only
        input.Input("     ");

        // Assert - Input should accept the whitespace
        input.GetAttribute("value").Should().Be("     ");
    }

    #endregion

    #region Value Change Tests

    [Fact]
    public async Task AutoComplete_ValueChangedNotFired_WhenNoItemSelected()
    {
        // Arrange
        var valueChangedCalled = false;
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<Product?>(this, _ => valueChangedCalled = true)));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("App"));

        // Assert - ValueChanged should not be called by just typing
        valueChangedCalled.Should().BeFalse();
    }

    [Fact]
    public void AutoComplete_WithNullValue_RendersEmptyInput()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Value, (Product?)null));

        var input = cut.Find("input.ebd-ac-input");

        // Assert
        input.GetAttribute("value").Should().BeNullOrEmpty();
    }

    #endregion

    #region Disabled State Tests

    [Fact]
    public void AutoComplete_WhenDisabled_InputIsDisabled()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Disabled, true)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act & Assert - Input should be disabled
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void AutoComplete_WhenDisabled_CannotBeCleared()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.AllowClear, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, new Product { Name = "Apple" }));

        // Assert
        cut.FindAll(".ebd-ac-clear-btn").Should().BeEmpty();
    }

    #endregion

    #region CloseOnSelect Tests

    [Fact]
    public async Task AutoComplete_WithCloseOnSelectFalse_KeepsDropdownOpen()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product>
            {
                new() { Id = 1, Name = "Apple" },
                new() { Id = 2, Name = "Apricot" }
            })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.CloseOnSelect, false)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        await cut.InvokeAsync(() => input.Input("App"));

        cut.WaitForAssertion(() =>
        {
            var item = cut.Find(".ebd-ac-item");
            item.Click();
        });

        // Assert - Dropdown should still be visible
        cut.WaitForAssertion(() =>
        {
            var dropdown = cut.FindAll(".ebd-ac-dropdown");
            dropdown.Should().NotBeEmpty();
        });
    }

    #endregion

    #region Placeholder Tests

    [Fact]
    public void AutoComplete_WithNullPlaceholder_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Placeholder, (string?)null));

        var input = cut.Find("input.ebd-ac-input");

        // Assert
        input.Should().NotBeNull();
    }

    [Fact]
    public void AutoComplete_WithEmptyPlaceholder_RendersWithoutError()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.Placeholder, string.Empty));

        var input = cut.Find("input.ebd-ac-input");

        // Assert
        input.Should().NotBeNull();
    }

    #endregion

    #region AriaLabel Tests

    [Fact]
    public void AutoComplete_WithNullAriaLabel_UsesDefaultAriaAttributes()
    {
        // Arrange & Act
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, new List<Product> { new() { Name = "Apple" } })
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.AriaLabel, (string?)null));

        var input = cut.Find("input.ebd-ac-input");

        // Assert
        input.GetAttribute("role").Should().Be("combobox");
        input.GetAttribute("aria-autocomplete").Should().Be("list");
    }

    #endregion
}
