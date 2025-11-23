using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EasyAppDev.Blazor.AutoComplete.Rendering;

/// <summary>
/// Renders items in simple text mode - just shows the main text field.
/// </summary>
/// <typeparam name="TItem">The type of items to render</typeparam>
public class SimpleRenderer<TItem> : IDisplayModeRenderer<TItem>
{
    public RenderFragment Render(TItem item, DisplayModeRenderContext<TItem> context) => builder =>
    {
        builder.OpenElement(0, "span");
        builder.AddContent(1, context.GetItemText(item));
        builder.CloseElement();
    };
}
