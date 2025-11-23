using EasyAppDev.Blazor.AutoComplete.Rendering;
using EasyAppDev.Blazor.AutoComplete.Options;
using FluentAssertions;
using Xunit;

namespace AutoComplete.Tests;

/// <summary>
/// Unit tests for display mode renderers and the renderer factory.
/// </summary>
public class DisplayModeRendererTests
{
    #region DisplayModeRendererFactory Tests

    [Theory]
    [InlineData(ItemDisplayMode.Simple, typeof(SimpleRenderer<Product>))]
    [InlineData(ItemDisplayMode.TitleWithDescription, typeof(TitleWithDescriptionRenderer<Product>))]
    [InlineData(ItemDisplayMode.TitleWithBadge, typeof(TitleWithBadgeRenderer<Product>))]
    [InlineData(ItemDisplayMode.TitleDescriptionBadge, typeof(TitleDescriptionBadgeRenderer<Product>))]
    [InlineData(ItemDisplayMode.IconWithTitle, typeof(IconWithTitleRenderer<Product>))]
    [InlineData(ItemDisplayMode.IconTitleDescription, typeof(IconTitleDescriptionRenderer<Product>))]
    [InlineData(ItemDisplayMode.Card, typeof(CardRenderer<Product>))]
    public void GetRenderer_ReturnsCorrectRendererType_ForEachDisplayMode(ItemDisplayMode mode, Type expectedType)
    {
        // Act
        var renderer = DisplayModeRendererFactory.GetRenderer<Product>(mode);

        // Assert
        renderer.Should().NotBeNull();
        renderer.Should().BeOfType(expectedType);
    }

    [Fact]
    public void GetRenderer_ReturnsFallbackRenderer_ForCustomMode()
    {
        // Act
        var renderer = DisplayModeRendererFactory.GetRenderer<Product>(ItemDisplayMode.Custom);

        // Assert
        renderer.Should().NotBeNull();
        renderer.Should().BeOfType<SimpleRenderer<Product>>();
    }

    [Fact]
    public void GetRenderer_ReturnsFallbackRenderer_ForInvalidMode()
    {
        // Arrange
        var invalidMode = (ItemDisplayMode)999;

        // Act
        var renderer = DisplayModeRendererFactory.GetRenderer<Product>(invalidMode);

        // Assert
        renderer.Should().NotBeNull();
        renderer.Should().BeOfType<SimpleRenderer<Product>>();
    }

    [Fact]
    public void SimpleRenderer_RendersItemText()
    {
        // Arrange
        var renderer = new SimpleRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    [Fact]
    public void TitleWithDescriptionRenderer_RendersWithContext()
    {
        // Arrange
        var renderer = new TitleWithDescriptionRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product", Category = "Category" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name,
            GetDescriptionText = p => p.Category
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    [Fact]
    public void TitleWithBadgeRenderer_RendersWithContext()
    {
        // Arrange
        var renderer = new TitleWithBadgeRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name,
            GetBadgeText = p => "New"
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    [Fact]
    public void TitleDescriptionBadgeRenderer_RendersWithContext()
    {
        // Arrange
        var renderer = new TitleDescriptionBadgeRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product", Category = "Category" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name,
            GetDescriptionText = p => p.Category,
            GetBadgeText = p => "New"
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    [Fact]
    public void IconWithTitleRenderer_RendersWithContext()
    {
        // Arrange
        var renderer = new IconWithTitleRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name,
            GetIconText = p => "📱"
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    [Fact]
    public void IconTitleDescriptionRenderer_RendersWithContext()
    {
        // Arrange
        var renderer = new IconTitleDescriptionRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product", Category = "Category" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name,
            GetDescriptionText = p => p.Category,
            GetIconText = p => "📱"
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    [Fact]
    public void CardRenderer_RendersWithContext()
    {
        // Arrange
        var renderer = new CardRenderer<Product>();
        var product = new Product { Id = 1, Name = "Test Product", Category = "Category" };
        var context = new DisplayModeRenderContext<Product>
        {
            GetItemText = p => p.Name,
            GetDescriptionText = p => p.Category,
            GetIconText = p => "📱",
            GetBadgeText = p => "New",
            GetSubtitleText = p => p.Category
        };

        // Act
        var renderFragment = renderer.Render(product, context);

        // Assert
        renderFragment.Should().NotBeNull();
    }

    #endregion
}
