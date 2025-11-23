using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items with an icon/emoji on the left and title.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class IconWithTitleRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "ebd-ac-icon-title");

        if (context.GetIconText?.Invoke(item) is string icon)
        {
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", "ebd-ac-icon");
            builder.AddContent(4, icon);
            builder.CloseElement();
        }

        builder.OpenElement(5, "span");
        builder.AddAttribute(6, "class", "ebd-ac-title");
        builder.AddContent(7, context.GetItemText(item));
        builder.CloseElement();

        builder.CloseElement();
    };
}
