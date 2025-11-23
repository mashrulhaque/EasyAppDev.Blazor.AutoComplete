namespace EasyAppDev.Blazor.AutoComplete.Options;

/// <summary>
/// Available filtering strategies for the AutoComplete component.
/// </summary>
public enum FilterStrategy
{
    /// <summary>
    /// Match items that start with the search text.
    /// This is the fastest filtering strategy.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Match items that contain the search text anywhere.
    /// Provides more flexible matching than StartsWith.
    /// </summary>
    Contains,

    /// <summary>
    /// Fuzzy matching with typo tolerance using Levenshtein distance.
    /// Best for user-friendly search with typo correction.
    /// </summary>
    Fuzzy,

    /// <summary>
    /// Custom filtering logic provided by the user.
    /// Use this when you need specialized filtering behavior.
    /// </summary>
    Custom
}
