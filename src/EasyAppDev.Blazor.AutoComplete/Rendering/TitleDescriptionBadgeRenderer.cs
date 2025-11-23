using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items with title, description below, and badge on the right.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class TitleDescriptionBadgeRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "ebd-ac-title-desc-badge");

        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "ebd-ac-content");

        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "ebd-ac-title");
        builder.AddContent(6, context.GetItemText(item));
        builder.CloseElement();

        if (context.GetDescriptionText?.Invoke(item) is string desc)
        {
            builder.OpenElement(7, "small");
            builder.AddAttribute(8, "class", "ebd-ac-description text-muted");
            builder.AddContent(9, desc);
            builder.CloseElement();
        }

        builder.CloseElement(); // close ebd-ac-content

        if (context.GetBadgeText?.Invoke(item) is string badge)
        {
            builder.OpenElement(10, "span");
            builder.AddAttribute(11, "class", $"{context.BadgeClass} ebd-ac-badge");
            builder.AddContent(12, badge);
            builder.CloseElement();
        }

        builder.CloseElement(); // close ebd-ac-title-desc-badge
    };
}
