using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items with icon/emoji, title, and description below.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class IconTitleDescriptionRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "ebd-ac-icon-title-desc");

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
        builder.AddAttribute(8, "class", "ebd-ac-title");
        builder.AddContent(9, context.GetItemText(item));
        builder.CloseElement();

        if (context.GetDescriptionText?.Invoke(item) is string desc)
        {
            builder.OpenElement(10, "small");
            builder.AddAttribute(11, "class", "ebd-ac-description text-muted");
            builder.AddContent(12, desc);
            builder.CloseElement();
        }

        builder.CloseElement(); // close ebd-ac-content
        builder.CloseElement(); // close ebd-ac-icon-title-desc
    };
}
