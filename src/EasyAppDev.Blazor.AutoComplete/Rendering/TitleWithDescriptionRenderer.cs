using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items with title (bold) and description (muted) below.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class TitleWithDescriptionRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "ebd-ac-title-desc");

        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "ebd-ac-title");
        builder.AddContent(4, context.GetItemText(item));
        builder.CloseElement();

        if (context.GetDescriptionText?.Invoke(item) is string desc)
        {
            builder.OpenElement(5, "small");
            builder.AddAttribute(6, "class", "ebd-ac-description text-muted");
            builder.AddContent(7, desc);
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}
