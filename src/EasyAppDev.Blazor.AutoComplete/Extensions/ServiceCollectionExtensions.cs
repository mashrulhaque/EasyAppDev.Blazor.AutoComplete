using EasyAppDev.Blazor.AutoComplete.Theming;
using EasyAppDev.Blazor.AutoComplete.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace EasyAppDev.Blazor.AutoComplete.Extensions;

/// <summary>
/// Extension methods for configuring AutoComplete services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AutoComplete services to the dependency injection container.
    /// This enables dependency injection for internal services like IThemeManager.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Services registered:
    /// - IThemeManager (Singleton): Theme management service for CSS class and property generation.
    ///
    /// Usage:
    /// <code>
    /// builder.Services.AddAutoComplete();
    /// </code>
    ///
    /// The AutoComplete component will automatically use registered services when available,
    /// or fall back to creating instances internally if not registered.
    /// </remarks>
    public static IServiceCollection AddAutoComplete(this IServiceCollection services)
    {
        // Use TryAdd to avoid overwriting existing registrations
        services.TryAddSingleton<IThemeManager, ThemeManager>();

        return services;
    }

    /// <summary>
    /// Adds AutoComplete services with a custom theme manager implementation.
    /// Useful for testing or custom theming behavior.
    /// </summary>
    /// <typeparam name="TThemeManager">The custom theme manager implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Usage:
    /// <code>
    /// builder.Services.AddAutoComplete&lt;CustomThemeManager&gt;();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddAutoComplete<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TThemeManager>(this IServiceCollection services)
        where TThemeManager : class, IThemeManager
    {
        services.AddSingleton<IThemeManager, TThemeManager>();
        return services;
    }

    /// <summary>
    /// Adds AutoComplete services with a custom theme manager factory.
    /// Useful for complex initialization scenarios.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="implementationFactory">Factory function to create the theme manager.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Usage:
    /// <code>
    /// builder.Services.AddAutoComplete(sp => new CustomThemeManager(sp.GetRequiredService&lt;IOptions&lt;ThemeConfig&gt;&gt;()));
    /// </code>
    /// </remarks>
    public static IServiceCollection AddAutoComplete(
        this IServiceCollection services,
        Func<IServiceProvider, IThemeManager> implementationFactory)
    {
        services.AddSingleton(implementationFactory);
        return services;
    }
}
