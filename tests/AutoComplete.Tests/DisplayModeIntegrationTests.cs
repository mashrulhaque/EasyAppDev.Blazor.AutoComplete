using EasyAppDev.Blazor.AutoComplete;
using EasyAppDev.Blazor.AutoComplete.Options;
using FluentAssertions;
using Xunit;

namespace AutoComplete.Tests;

/// <summary>
/// Integration tests for display modes in the AutoComplete component.
/// </summary>
public class DisplayModeIntegrationTests : AutoCompleteTestBase
{
    private readonly List<ProductWithMetadata> _testProducts = new()
    {
        new()
        {
            Id = 1,
            Name = "Apple iPhone",
            Description = "Latest smartphone",
            Icon = "📱",
            Category = "Electronics",
            Badge = "New"
        },
        new()
        {
            Id = 2,
            Name = "Samsung Galaxy",
            Description = "Android flagship",
            Icon = "📱",
            Category = "Electronics",
            Badge = "Sale"
        }
    };

    #region Simple Display Mode Tests

    [Fact]
    public void AutoComplete_SimpleDisplayMode_RendersCorrectly()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.Simple)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert - Simple mode should render items
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
            items[0].TextContent.Should().Contain("Apple iPhone");
        });
    }

    #endregion

    #region TitleWithDescription Display Mode Tests

    [Fact]
    public void AutoComplete_TitleWithDescriptionMode_RendersTitle()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.TitleWithDescription)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var title = cut.Find(".ebd-ac-title");
            title.TextContent.Should().Contain("Apple iPhone");
        });
    }

    [Fact]
    public void AutoComplete_TitleWithDescriptionMode_RendersDescription()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.TitleWithDescription)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var description = cut.Find(".ebd-ac-description");
            description.TextContent.Should().Contain("Latest smartphone");
        });
    }

    [Fact]
    public void AutoComplete_TitleWithDescriptionMode_AppliesCorrectCssClass()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.TitleWithDescription)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert - The CSS class is on the content div, not the item wrapper
        cut.WaitForAssertion(() =>
        {
            var titleDesc = cut.Find(".ebd-ac-title-desc");
            titleDesc.Should().NotBeNull();
        });
    }

    #endregion

    #region TitleWithBadge Display Mode Tests

    [Fact]
    public void AutoComplete_TitleWithBadgeMode_RendersBadge()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.TitleWithBadge)
            .Add(p => p.BadgeField, p => p.Badge)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var badge = cut.Find(".ebd-ac-badge");
            badge.TextContent.Should().Contain("New");
        });
    }

    [Fact]
    public void AutoComplete_TitleWithBadgeMode_AppliesBadgeClass()
    {
        // Arrange
        var customBadgeClass = "custom-badge-class";
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.TitleWithBadge)
            .Add(p => p.BadgeField, p => p.Badge)
            .Add(p => p.BadgeClass, customBadgeClass)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var badge = cut.Find(".ebd-ac-badge");
            badge.ClassList.Should().Contain(customBadgeClass);
        });
    }

    #endregion

    #region TitleDescriptionBadge Display Mode Tests

    [Fact]
    public void AutoComplete_TitleDescriptionBadgeMode_RendersAllElements()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.TitleDescriptionBadge)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.BadgeField, p => p.Badge)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var title = cut.Find(".ebd-ac-title");
            var description = cut.Find(".ebd-ac-description");
            var badge = cut.Find(".ebd-ac-badge");

            title.TextContent.Should().Contain("Apple iPhone");
            description.TextContent.Should().Contain("Latest smartphone");
            badge.TextContent.Should().Contain("New");
        });
    }

    #endregion

    #region IconWithTitle Display Mode Tests

    [Fact]
    public void AutoComplete_IconWithTitleMode_RendersIcon()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.IconWithTitle)
            .Add(p => p.IconField, p => p.Icon)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var icon = cut.Find(".ebd-ac-icon");
            icon.TextContent.Should().Contain("📱");
        });
    }

    [Fact]
    public void AutoComplete_IconWithTitleMode_RendersTitle()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.IconWithTitle)
            .Add(p => p.IconField, p => p.Icon)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var title = cut.Find(".ebd-ac-title");
            title.TextContent.Should().Contain("Apple iPhone");
        });
    }

    #endregion

    #region IconTitleDescription Display Mode Tests

    [Fact]
    public void AutoComplete_IconTitleDescriptionMode_RendersAllElements()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.IconTitleDescription)
            .Add(p => p.IconField, p => p.Icon)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var icon = cut.Find(".ebd-ac-icon");
            var title = cut.Find(".ebd-ac-title");
            var description = cut.Find(".ebd-ac-description");

            icon.TextContent.Should().Contain("📱");
            title.TextContent.Should().Contain("Apple iPhone");
            description.TextContent.Should().Contain("Latest smartphone");
        });
    }

    #endregion

    #region Card Display Mode Tests

    [Fact]
    public void AutoComplete_CardMode_RendersAllAvailableFields()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.Card)
            .Add(p => p.IconField, p => p.Icon)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.BadgeField, p => p.Badge)
            .Add(p => p.SubtitleField, p => p.Category)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var icon = cut.Find(".ebd-ac-icon");
            var title = cut.Find(".ebd-ac-title");
            var subtitle = cut.Find(".ebd-ac-subtitle");
            var description = cut.Find(".ebd-ac-description");
            var badge = cut.Find(".ebd-ac-badge");

            icon.TextContent.Should().Contain("📱");
            title.TextContent.Should().Contain("Apple iPhone");
            subtitle.TextContent.Should().Contain("Electronics");
            description.TextContent.Should().Contain("Latest smartphone");
            badge.TextContent.Should().Contain("New");
        });
    }

    [Fact]
    public void AutoComplete_CardMode_HandlesPartialFields()
    {
        // Arrange - Only provide title and description, no icon or badge
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.Card)
            .Add(p => p.DescriptionField, p => p.Description)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert - Should render without icon and badge
        cut.WaitForAssertion(() =>
        {
            var title = cut.Find(".ebd-ac-title");
            var description = cut.Find(".ebd-ac-description");

            title.TextContent.Should().Contain("Apple iPhone");
            description.TextContent.Should().Contain("Latest smartphone");

            // Icon and badge should not be present
            cut.FindAll(".ebd-ac-icon").Should().BeEmpty();
            cut.FindAll(".ebd-ac-badge").Should().BeEmpty();
        });
    }

    #endregion

    #region Custom Display Mode Tests

    [Fact]
    public void AutoComplete_CustomDisplayMode_UsesItemTemplate()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.Custom)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-item-template");
                builder.AddContent(2, $"Custom: {item.Name}");
                builder.CloseElement();
            })
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert
        cut.WaitForAssertion(() =>
        {
            var customItem = cut.Find(".custom-item-template");
            customItem.TextContent.Should().Contain("Custom: Apple iPhone");
        });
    }

    [Fact]
    public void AutoComplete_CustomDisplayMode_FallsBackToSimpleWhenNoTemplate()
    {
        // Arrange
        var cut = RenderComponent<AutoComplete<ProductWithMetadata>>(parameters => parameters
            .Add(p => p.Items, _testProducts)
            .Add(p => p.TextField, p => p.Name)
            .Add(p => p.DisplayMode, ItemDisplayMode.Custom)
            .Add(p => p.DebounceMs, 0));

        var input = cut.Find("input.ebd-ac-input");

        // Act
        input.Input("Apple");

        // Assert - Should render items even without custom template
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".ebd-ac-item");
            items.Should().ContainSingle();
            items[0].TextContent.Should().Contain("Apple iPhone");
        });
    }

    #endregion
}

/// <summary>
/// Extended product model with additional fields for display mode testing.
/// </summary>
public class ProductWithMetadata
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
}
