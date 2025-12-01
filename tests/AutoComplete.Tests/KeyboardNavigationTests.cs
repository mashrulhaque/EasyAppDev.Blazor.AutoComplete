using EasyAppDev.Blazor.AutoComplete;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for keyboard navigation functionality
/// </summary>
public class KeyboardNavigationTests : AutoCompleteTestBase
{
    private readonly List<Product> _testProducts = new()
    {
        new Product { Id = 1, Name = "Apple", Category = "Fruit" },
        new Product { Id = 2, Name = "Banana", Category = "Fruit" },
        new Product { Id = 3, Name = "Cherry", Category = "Fruit" },
        new Product { Id = 4, Name = "Date", Category = "Fruit" }
    };

    [Fact]
    public async Task AutoComplete_ArrowDown_SelectsNextItem()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("a")); // Match all items

        // Wait for items to appear
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(0);
        });

        // Act - Press ArrowDown to select first item
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));

        // Assert - First item should have keyboard-selected class
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items[0].ClassList.Should().Contain("keyboard-selected");
        });
    }

    [Fact]
    public async Task AutoComplete_ArrowUp_SelectsPreviousItem()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 0)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("")); // Empty search shows all items

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(1);
        });

        // Act - Press ArrowDown twice then ArrowUp once
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" }));

        // Assert - First item should be selected again
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items[0].ClassList.Should().Contain("keyboard-selected");
        });
    }

    [Fact]
    public async Task AutoComplete_Enter_SelectsHighlightedItem()
    {
        // Arrange
        Product? selectedProduct = null;
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<Product?>(this, value => selectedProduct = value)));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("a"));

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(0);
        });

        // Act - Navigate down and press Enter
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "Enter" }));

        // Assert
        selectedProduct.Should().NotBeNull();
        selectedProduct!.Name.Should().Be("Apple");
    }

    [Fact]
    public async Task AutoComplete_Escape_ClosesDropdown()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("a"));

        cut.WaitForAssertion(() =>
        {
            cut.Find(".ebd-ac-dropdown").Should().NotBeNull();
        });

        // Act - Press Escape
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "Escape" }));

        // Assert - Dropdown should be closed
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-dropdown").Should().BeEmpty();
        });
    }

    [Fact]
    public async Task AutoComplete_Home_SelectsFirstItem()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 0)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("")); // Empty search shows all items

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(2);
        });

        // Act - Navigate down a few times, then press Home
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "Home" }));

        // Assert - First item should be selected
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items[0].ClassList.Should().Contain("keyboard-selected");
        });
    }

    [Fact]
    public async Task AutoComplete_End_SelectsLastItem()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 0)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("")); // Empty search shows all items

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(2);
        });

        // Act - Press End
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "End" }));

        // Assert - Last item should be selected
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items[^1].ClassList.Should().Contain("keyboard-selected");
        });
    }

    [Fact]
    public async Task AutoComplete_UpdatesAriaActivedescendant()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("a"));

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(0);
        });

        // Act - Navigate with keyboard
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));

        // Assert - aria-activedescendant should be set
        cut.WaitForAssertion(() =>
        {
            input = cut.Find("input.ebd-ac-input");
            var ariaDescendant = input.GetAttribute("aria-activedescendant");
            ariaDescendant.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task AutoComplete_MouseEnter_UpdatesKeyboardSelection()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<Product>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.MinSearchLength, 0)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");
        await cut.InvokeAsync(() => input.Input("")); // Empty search shows all items

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ebd-ac-item").Should().HaveCountGreaterThan(1);
        });

        // Act - First navigate with keyboard, then hover over different item
        await cut.InvokeAsync(() => input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" }));

        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items[1].TriggerEvent("onmouseenter", EventArgs.Empty); // Hover over second item
        });

        // Assert - Second item should now be keyboard-selected
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items[1].ClassList.Should().Contain("keyboard-selected");
        });
    }
}
