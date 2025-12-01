using Bunit;
using EasyAppDev.Blazor.AutoComplete.Extensions;
using EasyAppDev.Blazor.AutoComplete.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace AutoComplete.Tests;

/// <summary>
/// Base test class for AutoComplete component tests.
/// Automatically registers required services for bUnit testing.
/// </summary>
public abstract class AutoCompleteTestBase : TestContext
{
    protected AutoCompleteTestBase()
    {
        // Register AutoComplete services (including IThemeManager)
        Services.AddAutoComplete();
    }
}
