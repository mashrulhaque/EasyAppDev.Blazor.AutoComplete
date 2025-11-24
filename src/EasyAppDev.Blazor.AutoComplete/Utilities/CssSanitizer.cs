using System.Text.RegularExpressions;

namespace EasyAppDev.Blazor.AutoComplete.Utilities;

/// <summary>
/// Provides CSS value sanitization to prevent injection attacks.
/// Uses allowlist-based validation for colors, lengths, fonts, and shadows.
/// </summary>
public static class CssSanitizer
{
    // Regex patterns for allowed CSS value formats
    private static readonly Regex HexColorPattern = new(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);
    private static readonly Regex RgbColorPattern = new(@"^rgba?\(\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}\s*(?:,\s*(?:0?\.\d+|1(?:\.0+)?)\s*)?\)$", RegexOptions.Compiled);
    private static readonly Regex HslColorPattern = new(@"^hsla?\(\s*\d{1,3}\s*,\s*\d{1,3}%\s*,\s*\d{1,3}%\s*(?:,\s*(?:0?\.\d+|1(?:\.0+)?)\s*)?\)$", RegexOptions.Compiled);
    private static readonly Regex LengthPattern = new(@"^-?\d+(?:\.\d+)?(?:px|em|rem|%|vh|vw|vmin|vmax|ch|ex|cm|mm|in|pt|pc)$", RegexOptions.Compiled);
    private static readonly Regex NumberPattern = new(@"^-?\d+(?:\.\d+)?$", RegexOptions.Compiled);
    private static readonly Regex TimePattern = new(@"^\d+(?:\.\d+)?(?:ms|s)$", RegexOptions.Compiled);
    private static readonly Regex SafeFontPattern = new(@"^[\w\s\-,'""]+(,\s*[\w\s\-,""']+)*$", RegexOptions.Compiled);

    // Allowed CSS named colors (subset of safe, common colors)
    private static readonly HashSet<string> AllowedNamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "transparent", "inherit", "currentColor",
        "black", "white", "gray", "silver",
        "red", "green", "blue", "yellow", "orange", "purple", "pink",
        "brown", "cyan", "magenta", "lime", "navy", "teal", "olive",
        "maroon", "aqua", "fuchsia"
    };

    // Allowed font family generic names
    private static readonly HashSet<string> AllowedGenericFonts = new(StringComparer.OrdinalIgnoreCase)
    {
        "serif", "sans-serif", "monospace", "cursive", "fantasy",
        "system-ui", "ui-serif", "ui-sans-serif", "ui-monospace", "ui-rounded"
    };

    // Dangerous patterns that should always be rejected
    private static readonly string[] DangerousPatterns = new[]
    {
        "url(", "expression(", "javascript:", "@import", "binding:",
        "<", ">", "{", "}", ";", "\\", "/*", "*/", "--"
    };

    /// <summary>
    /// Sanitizes a CSS color value (hex, rgb, hsl, or named color).
    /// Returns null if the value is invalid or dangerous.
    /// </summary>
    public static string? SanitizeColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        // Check for dangerous patterns
        if (ContainsDangerousPattern(trimmed))
        {
            return null;
        }

        // Check hex colors
        if (HexColorPattern.IsMatch(trimmed))
        {
            return trimmed;
        }

        // Check rgb/rgba
        if (RgbColorPattern.IsMatch(trimmed))
        {
            return ValidateRgbValues(trimmed);
        }

        // Check hsl/hsla
        if (HslColorPattern.IsMatch(trimmed))
        {
            return ValidateHslValues(trimmed);
        }

        // Check named colors
        if (AllowedNamedColors.Contains(trimmed))
        {
            return trimmed;
        }

        return null;
    }

    /// <summary>
    /// Sanitizes a CSS length value (px, em, rem, %, vh, vw, etc.).
    /// Supports multiple values (e.g., "10px 20px" for padding).
    /// Returns null if the value is invalid or dangerous.
    /// </summary>
    public static string? SanitizeLength(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        // Check for dangerous patterns
        if (ContainsDangerousPattern(trimmed))
        {
            return null;
        }

        // Split by whitespace to check multiple values (e.g., "10px 20px" for padding)
        var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            // Check length pattern for each part
            if (!LengthPattern.IsMatch(part) && part != "0")
            {
                return null;
            }
        }

        return trimmed;
    }

    /// <summary>
    /// Sanitizes a font family value.
    /// Returns null if the value is invalid or dangerous.
    /// </summary>
    public static string? SanitizeFontFamily(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        // Check for dangerous patterns
        if (ContainsDangerousPattern(trimmed))
        {
            return null;
        }

        // Check if it matches safe font pattern
        if (!SafeFontPattern.IsMatch(trimmed))
        {
            return null;
        }

        // Validate each font in the stack
        var fonts = trimmed.Split(',');
        foreach (var font in fonts)
        {
            var fontName = font.Trim().Trim('"', '\'');

            // Allow generic font families
            if (AllowedGenericFonts.Contains(fontName))
            {
                continue;
            }

            // Allow alphanumeric font names with spaces and hyphens
            if (!Regex.IsMatch(fontName, @"^[\w\s\-]+$"))
            {
                return null;
            }
        }

        return trimmed;
    }

    /// <summary>
    /// Sanitizes a CSS shadow value (box-shadow, text-shadow).
    /// Returns null if the value is invalid or dangerous.
    /// </summary>
    public static string? SanitizeShadow(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        // Check for dangerous patterns
        if (ContainsDangerousPattern(trimmed))
        {
            return null;
        }

        // Allow "none"
        if (trimmed.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        // Shadow format: h-offset v-offset blur spread color [inset]
        // Example: 0 4px 6px rgba(0, 0, 0, 0.1)
        var parts = Regex.Split(trimmed, @"\s+");

        // Must have at least 3 parts (h-offset, v-offset, color)
        if (parts.Length < 3)
        {
            return null;
        }

        // Validate each part is either a length, color, or "inset"
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            // Check if it's a valid length
            if (LengthPattern.IsMatch(part) || part == "0")
            {
                continue;
            }

            // Check if it's "inset"
            if (part.Equals("inset", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Check if it's a valid color (including multi-word like "rgba(...)")
            var colorStart = trimmed.IndexOf(part, StringComparison.Ordinal);
            var potentialColor = trimmed.Substring(colorStart);
            if (SanitizeColor(potentialColor) != null)
            {
                break; // Rest is color, stop checking
            }

            // If none of the above, it's invalid
            return null;
        }

        return trimmed;
    }

    /// <summary>
    /// Sanitizes a CSS time value (transition-duration, animation-duration).
    /// Returns null if the value is invalid or dangerous.
    /// </summary>
    public static string? SanitizeTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        // Check for dangerous patterns
        if (ContainsDangerousPattern(trimmed))
        {
            return null;
        }

        // Check time pattern
        if (TimePattern.IsMatch(trimmed))
        {
            return trimmed;
        }

        return null;
    }

    /// <summary>
    /// Sanitizes a generic CSS value (letter-spacing, line-height, etc.).
    /// Returns null if the value is invalid or dangerous.
    /// </summary>
    public static string? SanitizeGenericValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        // Check for dangerous patterns
        if (ContainsDangerousPattern(trimmed))
        {
            return null;
        }

        // Allow lengths
        if (LengthPattern.IsMatch(trimmed))
        {
            return trimmed;
        }

        // Allow plain numbers
        if (NumberPattern.IsMatch(trimmed))
        {
            return trimmed;
        }

        // Allow "normal", "inherit", "initial", "unset"
        var allowedKeywords = new[] { "normal", "inherit", "initial", "unset", "auto" };
        if (allowedKeywords.Any(k => k.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return trimmed;
        }

        return null;
    }

    /// <summary>
    /// Checks if a value contains any dangerous patterns.
    /// </summary>
    private static bool ContainsDangerousPattern(string value)
    {
        foreach (var pattern in DangerousPatterns)
        {
            if (value.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Validates RGB values are within 0-255 range.
    /// </summary>
    private static string? ValidateRgbValues(string rgb)
    {
        var numbers = Regex.Matches(rgb, @"\d+");
        if (numbers.Count < 3)
        {
            return null;
        }

        for (int i = 0; i < 3; i++)
        {
            if (int.TryParse(numbers[i].Value, out var val) && (val < 0 || val > 255))
            {
                return null;
            }
        }

        return rgb;
    }

    /// <summary>
    /// Validates HSL values are within valid ranges.
    /// </summary>
    private static string? ValidateHslValues(string hsl)
    {
        var numbers = Regex.Matches(hsl, @"\d+");
        if (numbers.Count < 3)
        {
            return null;
        }

        // Hue: 0-360
        if (int.TryParse(numbers[0].Value, out var hue) && (hue < 0 || hue > 360))
        {
            return null;
        }

        // Saturation and Lightness: 0-100%
        for (int i = 1; i < 3; i++)
        {
            if (int.TryParse(numbers[i].Value, out var val) && (val < 0 || val > 100))
            {
                return null;
            }
        }

        return hsl;
    }
}
