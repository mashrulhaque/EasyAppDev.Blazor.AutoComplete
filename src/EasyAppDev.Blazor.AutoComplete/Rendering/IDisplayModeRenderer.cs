using Microsoft.AspNetCore.Components;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Interface for rendering different display modes in the autocomplete dropdown.
/// Implements the Strategy pattern for flexible item rendering.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public interface IDisplayModeRenderer<TItem>
{
    /// <summary>
    /// Renders an item in the autocomplete dropdown.
    /// </summary>
    /// <param name="item">The item to render</param>
    /// <param name="context">Rendering context containing field accessors and configuration</param>
    /// <returns>A RenderFragment representing the rendered item</returns>
    RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context);
}

/// <summary>
/// Context object containing all necessary data for rendering an item.
/// </summary>
/// <typeparam name="TItem">The type of items being rendered</typeparam>
public class DisplayModeRenderContext<TItem>
{
    /// <summary>
    /// Function to extract the main text from an item.
    /// </summary>
    public required Func<TItem, string> GetItemText { get; init; }

    /// <summary>
    /// Function to extract the description text from an item (optional).
    /// </summary>
    public Func<TItem, string?>? GetDescriptionText { get; init; }

    /// <summary>
    /// Function to extract the badge text from an item (optional).
    /// </summary>
    public Func<TItem, string?>? GetBadgeText { get; init; }

    /// <summary>
    /// Function to extract the icon text from an item (optional).
    /// </summary>
    public Func<TItem, string?>? GetIconText { get; init; }

    /// <summary>
    /// Function to extract the subtitle text from an item (optional).
    /// </summary>
    public Func<TItem, string?>? GetSubtitleText { get; init; }

    /// <summary>
    /// CSS class to apply to badge elements.
    /// </summary>
    public string? BadgeClass { get; init; }
}
