using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items with title and a right-aligned badge.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class TitleWithBadgeRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "ebd-ac-title-badge");

        builder.OpenElement(2, "span");
        builder.AddAttribute(3, "class", "ebd-ac-title");
        builder.AddContent(4, context.GetItemText(item));
        builder.CloseElement();

        if (context.GetBadgeText?.Invoke(item) is string badge)
        {
            builder.OpenElement(5, "span");
            builder.AddAttribute(6, "class", $"{context.BadgeClass} ebd-ac-badge");
            builder.AddContent(7, badge);
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}
