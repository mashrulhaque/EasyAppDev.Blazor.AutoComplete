using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items in card-style format with all available fields.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class CardRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "ebd-ac-card");

        if (context.GetIconText?.Invoke(item) is string icon)
        {
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", "ebd-ac-icon");
            builder.AddContent(4, icon);
            builder.CloseElement();
        }

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", "ebd-ac-content");

        builder.OpenElement(7, "div");
        builder.AddAttribute(8, "class", "ebd-ac-header");

        builder.OpenElement(9, "span");
        builder.AddAttribute(10, "class", "ebd-ac-title");
        builder.AddContent(11, context.GetItemText(item));
        builder.CloseElement();

        if (context.GetBadgeText?.Invoke(item) is string badge)
        {
            builder.OpenElement(12, "span");
            builder.AddAttribute(13, "class", $"{context.BadgeClass} ebd-ac-badge");
            builder.AddContent(14, badge);
            builder.CloseElement();
        }

        builder.CloseElement(); // close ebd-ac-header

        if (context.GetSubtitleText?.Invoke(item) is string subtitle)
        {
            builder.OpenElement(15, "div");
            builder.AddAttribute(16, "class", "ebd-ac-subtitle text-muted");
            builder.AddContent(17, subtitle);
            builder.CloseElement();
        }

        if (context.GetDescriptionText?.Invoke(item) is string desc)
        {
            builder.OpenElement(18, "small");
            builder.AddAttribute(19, "class", "ebd-ac-description text-muted");
            builder.AddContent(20, desc);
            builder.CloseElement();
        }

        builder.CloseElement(); // close ebd-ac-content
        builder.CloseElement(); // close ebd-ac-card
    };
}
