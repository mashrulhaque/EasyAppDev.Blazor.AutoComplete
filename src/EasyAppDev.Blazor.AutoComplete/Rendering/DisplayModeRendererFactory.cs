using EasyAppDev.Blazor.AutoComplete.Options;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Factory for creating display mode renderers based on the specified mode.
/// </summary>
public static class DisplayModeRendererFactory
{
    /// <summary>
    /// Gets the appropriate renderer for the specified display mode.
    /// </summary>
    /// <typeparam name="TItem">The type of items to render</typeparam>
    /// <param name="mode">The display mode</param>
    /// <returns>A renderer instance for the specified mode</returns>
    public static IDisplayModeRenderer<TItem> GetRenderer<TItem>(ItemDisplayMode mode)
    {
        return mode switch
        {
            ItemDisplayMode.Simple => new SimpleRenderer<TItem>(),
            ItemDisplayMode.TitleWithDescription => new TitleWithDescriptionRenderer<TItem>(),
            ItemDisplayMode.TitleWithBadge => new TitleWithBadgeRenderer<TItem>(),
            ItemDisplayMode.TitleDescriptionBadge => new TitleDescriptionBadgeRenderer<TItem>(),
            ItemDisplayMode.IconWithTitle => new IconWithTitleRenderer<TItem>(),
            ItemDisplayMode.IconTitleDescription => new IconTitleDescriptionRenderer<TItem>(),
            ItemDisplayMode.Card => new CardRenderer<TItem>(),
            ItemDisplayMode.Custom => new SimpleRenderer<TItem>(), // Fallback to Simple for Custom mode
            _ => new SimpleRenderer<TItem>()
        };
    }
}
