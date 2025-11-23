using System;
using System.Globalization;

namespace EasyAppDev.Blazor.AutoComplete.Theming;

/// <summary>
/// Utility class for color manipulation and conversions
/// </summary>
internal static class ColorHelper
{
    /// <summary>
    /// Lightens a hex color by a specified percentage (0.0 to 1.0)
    /// </summary>
    /// <param name="hexColor">Hex color string (e.g., "#6200EE" or "6200EE")</param>
    /// <param name="amount">Amount to lighten (0.0 = no change, 1.0 = white). Default is 0.9 for a very light tint.</param>
    /// <returns>Lightened hex color string</returns>
    public static string Lighten(string hexColor, double amount = 0.9)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            return hexColor;
        }

        // Remove # if present
        hexColor = hexColor.TrimStart('#');

        // Handle 3-digit hex colors
        if (hexColor.Length == 3)
        {
            hexColor = $"{hexColor[0]}{hexColor[0]}{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}";
        }

        // Validate hex color
        if (hexColor.Length != 6 || !IsValidHex(hexColor))
        {
            return $"#{hexColor}"; // Return original if invalid
        }

        try
        {
            // Parse RGB components
            int r = int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
            int g = int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
            int b = int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);

            // Lighten by moving towards white (255, 255, 255)
            // Formula: newColor = currentColor + (255 - currentColor) * amount
            r = (int)(r + (255 - r) * amount);
            g = (int)(g + (255 - g) * amount);
            b = (int)(b + (255 - b) * amount);

            // Clamp values to 0-255
            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            // Convert back to hex
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch
        {
            // Return original color if parsing fails
            return $"#{hexColor}";
        }
    }

    /// <summary>
    /// Validates if a string is a valid hex color (without #)
    /// </summary>
    private static bool IsValidHex(string hex)
    {
        foreach (char c in hex)
        {
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                return false;
            }
        }
        return true;
    }
}
