using SketchPen.Plot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SketchPen.Compose.Services;

/// <summary>
/// Replaces literal colors in a generated SVG document with CSS custom property
/// references (<c>var(--sketchpen-&lt;globalsKey&gt;, &lt;originalValue&gt;)</c>), for every
/// color that came from a known <c>.globals</c> variable (<c>globals.set</c>/<c>tryset</c>).
/// </summary>
/// <remarks>
/// SkiaSharp's SVG writer does not echo back the original hex string SketchPen fed it —
/// e.g. pure black/white are written as the CSS keywords <c>black</c>/<c>white</c>, and
/// some hex codes are shortened (<c>#dddddd</c> → <c>#DDD</c>). Matching therefore has to
/// parse both sides to RGBA and compare numerically, not compare strings.
/// </remarks>
internal static class SvgColorVariableSubstitution
{
    private static readonly Regex ColorAttributeRegex =
        new Regex("(fill|stroke|stop-color)=\"([^\"]+)\"", RegexOptions.Compiled);

    public readonly struct UsedVariable
    {
        public UsedVariable(string key, string originalValue)
        {
            Key = key;
            OriginalValue = originalValue;
        }

        public string Key { get; }
        public string OriginalValue { get; }
    }

    /// <summary>
    /// Substitutes every <c>fill=</c>/<c>stroke=</c>/<c>stop-color=</c> attribute value in
    /// <paramref name="svg"/> whose color numerically matches a string-valued entry of
    /// <paramref name="globals"/> that itself parses as a color (via <see cref="PlotColor.FromHtml"/>).
    /// <c>fill="none"</c> and gradient references (<c>fill="url(#...)"</c>) are left untouched.
    /// </summary>
    /// <returns>
    /// The substituted SVG text, and the list of globals keys that were actually referenced
    /// (in <paramref name="globals"/> enumeration order, one entry per key even if it was
    /// matched more than once) — used to build a corresponding CSS <c>:root</c> block.
    /// </returns>
    public static (string Svg, IReadOnlyList<UsedVariable> UsedVariables) Substitute(
        string svg,
        IDictionary<string, object> globals,
        string variablePrefix = "sketchpen-")
    {
        var candidates = new List<(string Key, string Value, (byte A, byte R, byte G, byte B) Rgba)>();
        foreach (var kvp in globals)
        {
            if (kvp.Value is string stringValue)
            {
                var color = PlotColor.FromHtml(stringValue);
                if (!color.IsTransparent)
                {
                    candidates.Add((kvp.Key, stringValue, (color.A, color.R, color.G, color.B)));
                }
            }
        }

        var usedKeys = new HashSet<string>();
        var usedVariables = new List<UsedVariable>();

        string result = ColorAttributeRegex.Replace(svg, match =>
        {
            string attributeName = match.Groups[1].Value;
            string attributeValue = match.Groups[2].Value;

            if (attributeValue == "none" || attributeValue.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
            {
                return match.Value;
            }

            if (!TryParseColor(attributeValue, out var rgba))
            {
                return match.Value;
            }

            foreach (var candidate in candidates)
            {
                if (candidate.Rgba == rgba)
                {
                    if (usedKeys.Add(candidate.Key))
                    {
                        usedVariables.Add(new UsedVariable(candidate.Key, candidate.Value));
                    }

                    return $"{attributeName}=\"var(--{variablePrefix}{candidate.Key}, {attributeValue})\"";
                }
            }

            return match.Value;
        });

        return (result, usedVariables);
    }

    /// <summary>
    /// Parses an SVG/CSS color value: <c>#RGB</c>, <c>#RRGGBB</c>, <c>#RGBA</c>,
    /// <c>#RRGGBBAA</c> (case-insensitive), or one of the standard SVG/CSS3 named colors.
    /// Returns <see langword="false"/> for anything else (e.g. <c>url(...)</c>, <c>none</c>,
    /// <c>rgb(...)</c>, unrecognized names) — callers should leave the value untouched in
    /// that case, this is a safe/non-throwing "don't substitute" signal.
    /// </summary>
    public static bool TryParseColor(string value, out (byte A, byte R, byte G, byte B) rgba)
    {
        rgba = default;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value[0] == '#')
        {
            var hex = value.Substring(1);

            switch (hex.Length)
            {
                case 3:
                    if (TryHexNibble(hex[0], out var r3) && TryHexNibble(hex[1], out var g3) && TryHexNibble(hex[2], out var b3))
                    {
                        rgba = (255, (byte)(r3 * 17), (byte)(g3 * 17), (byte)(b3 * 17));
                        return true;
                    }
                    return false;

                case 4:
                    if (TryHexNibble(hex[0], out var r4) && TryHexNibble(hex[1], out var g4) &&
                        TryHexNibble(hex[2], out var b4) && TryHexNibble(hex[3], out var a4))
                    {
                        rgba = ((byte)(a4 * 17), (byte)(r4 * 17), (byte)(g4 * 17), (byte)(b4 * 17));
                        return true;
                    }
                    return false;

                case 6:
                    if (TryHexByte(hex, 0, out var r6) && TryHexByte(hex, 2, out var g6) && TryHexByte(hex, 4, out var b6))
                    {
                        rgba = (255, r6, g6, b6);
                        return true;
                    }
                    return false;

                case 8:
                    if (TryHexByte(hex, 0, out var r8) && TryHexByte(hex, 2, out var g8) &&
                        TryHexByte(hex, 4, out var b8) && TryHexByte(hex, 6, out var a8))
                    {
                        rgba = (a8, r8, g8, b8);
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        if (NamedColors.TryGetValue(value, out var named))
        {
            rgba = (255, named.R, named.G, named.B);
            return true;
        }

        return false;
    }

    private static bool TryHexNibble(char c, out int value)
    {
        if (c >= '0' && c <= '9') { value = c - '0'; return true; }
        if (c >= 'a' && c <= 'f') { value = c - 'a' + 10; return true; }
        if (c >= 'A' && c <= 'F') { value = c - 'A' + 10; return true; }
        value = 0;
        return false;
    }

    private static bool TryHexByte(string hex, int index, out byte value)
    {
        value = 0;
        if (!TryHexNibble(hex[index], out var hi) || !TryHexNibble(hex[index + 1], out var lo))
        {
            return false;
        }
        value = (byte)((hi << 4) | lo);
        return true;
    }

    // Standard SVG 1.1 / CSS Color Module Level 3 extended named colors, lowercase keys.
    private static readonly Dictionary<string, (byte R, byte G, byte B)> NamedColors =
        new Dictionary<string, (byte, byte, byte)>(StringComparer.OrdinalIgnoreCase)
        {
            ["aliceblue"] = (240, 248, 255),
            ["antiquewhite"] = (250, 235, 215),
            ["aqua"] = (0, 255, 255),
            ["aquamarine"] = (127, 255, 212),
            ["azure"] = (240, 255, 255),
            ["beige"] = (245, 245, 220),
            ["bisque"] = (255, 228, 196),
            ["black"] = (0, 0, 0),
            ["blanchedalmond"] = (255, 235, 205),
            ["blue"] = (0, 0, 255),
            ["blueviolet"] = (138, 43, 226),
            ["brown"] = (165, 42, 42),
            ["burlywood"] = (222, 184, 135),
            ["cadetblue"] = (95, 158, 160),
            ["chartreuse"] = (127, 255, 0),
            ["chocolate"] = (210, 105, 30),
            ["coral"] = (255, 127, 80),
            ["cornflowerblue"] = (100, 149, 237),
            ["cornsilk"] = (255, 248, 220),
            ["crimson"] = (220, 20, 60),
            ["cyan"] = (0, 255, 255),
            ["darkblue"] = (0, 0, 139),
            ["darkcyan"] = (0, 139, 139),
            ["darkgoldenrod"] = (184, 134, 11),
            ["darkgray"] = (169, 169, 169),
            ["darkgreen"] = (0, 100, 0),
            ["darkgrey"] = (169, 169, 169),
            ["darkkhaki"] = (189, 183, 107),
            ["darkmagenta"] = (139, 0, 139),
            ["darkolivegreen"] = (85, 107, 47),
            ["darkorange"] = (255, 140, 0),
            ["darkorchid"] = (153, 50, 204),
            ["darkred"] = (139, 0, 0),
            ["darksalmon"] = (233, 150, 122),
            ["darkseagreen"] = (143, 188, 143),
            ["darkslateblue"] = (72, 61, 139),
            ["darkslategray"] = (47, 79, 79),
            ["darkslategrey"] = (47, 79, 79),
            ["darkturquoise"] = (0, 206, 209),
            ["darkviolet"] = (148, 0, 211),
            ["deeppink"] = (255, 20, 147),
            ["deepskyblue"] = (0, 191, 255),
            ["dimgray"] = (105, 105, 105),
            ["dimgrey"] = (105, 105, 105),
            ["dodgerblue"] = (30, 144, 255),
            ["firebrick"] = (178, 34, 34),
            ["floralwhite"] = (255, 250, 240),
            ["forestgreen"] = (34, 139, 34),
            ["fuchsia"] = (255, 0, 255),
            ["gainsboro"] = (220, 220, 220),
            ["ghostwhite"] = (248, 248, 255),
            ["gold"] = (255, 215, 0),
            ["goldenrod"] = (218, 165, 32),
            ["gray"] = (128, 128, 128),
            ["grey"] = (128, 128, 128),
            ["green"] = (0, 128, 0),
            ["greenyellow"] = (173, 255, 47),
            ["honeydew"] = (240, 255, 240),
            ["hotpink"] = (255, 105, 180),
            ["indianred"] = (205, 92, 92),
            ["indigo"] = (75, 0, 130),
            ["ivory"] = (255, 255, 240),
            ["khaki"] = (240, 230, 140),
            ["lavender"] = (230, 230, 250),
            ["lavenderblush"] = (255, 240, 245),
            ["lawngreen"] = (124, 252, 0),
            ["lemonchiffon"] = (255, 250, 205),
            ["lightblue"] = (173, 216, 230),
            ["lightcoral"] = (240, 128, 128),
            ["lightcyan"] = (224, 255, 255),
            ["lightgoldenrodyellow"] = (250, 250, 210),
            ["lightgray"] = (211, 211, 211),
            ["lightgreen"] = (144, 238, 144),
            ["lightgrey"] = (211, 211, 211),
            ["lightpink"] = (255, 182, 193),
            ["lightsalmon"] = (255, 160, 122),
            ["lightseagreen"] = (32, 178, 170),
            ["lightskyblue"] = (135, 206, 250),
            ["lightslategray"] = (119, 136, 153),
            ["lightslategrey"] = (119, 136, 153),
            ["lightsteelblue"] = (176, 196, 222),
            ["lightyellow"] = (255, 255, 224),
            ["lime"] = (0, 255, 0),
            ["limegreen"] = (50, 205, 50),
            ["linen"] = (250, 240, 230),
            ["magenta"] = (255, 0, 255),
            ["maroon"] = (128, 0, 0),
            ["mediumaquamarine"] = (102, 205, 170),
            ["mediumblue"] = (0, 0, 205),
            ["mediumorchid"] = (186, 85, 211),
            ["mediumpurple"] = (147, 112, 219),
            ["mediumseagreen"] = (60, 179, 113),
            ["mediumslateblue"] = (123, 104, 238),
            ["mediumspringgreen"] = (0, 250, 154),
            ["mediumturquoise"] = (72, 209, 204),
            ["mediumvioletred"] = (199, 21, 133),
            ["midnightblue"] = (25, 25, 112),
            ["mintcream"] = (245, 255, 250),
            ["mistyrose"] = (255, 228, 225),
            ["moccasin"] = (255, 228, 181),
            ["navajowhite"] = (255, 222, 173),
            ["navy"] = (0, 0, 128),
            ["oldlace"] = (253, 245, 230),
            ["olive"] = (128, 128, 0),
            ["olivedrab"] = (107, 142, 35),
            ["orange"] = (255, 165, 0),
            ["orangered"] = (255, 69, 0),
            ["orchid"] = (218, 112, 214),
            ["palegoldenrod"] = (238, 232, 170),
            ["palegreen"] = (152, 251, 152),
            ["paleturquoise"] = (175, 238, 238),
            ["palevioletred"] = (219, 112, 147),
            ["papayawhip"] = (255, 239, 213),
            ["peachpuff"] = (255, 218, 185),
            ["peru"] = (205, 133, 63),
            ["pink"] = (255, 192, 203),
            ["plum"] = (221, 160, 221),
            ["powderblue"] = (176, 224, 230),
            ["purple"] = (128, 0, 128),
            ["rebeccapurple"] = (102, 51, 153),
            ["red"] = (255, 0, 0),
            ["rosybrown"] = (188, 143, 143),
            ["royalblue"] = (65, 105, 225),
            ["saddlebrown"] = (139, 69, 19),
            ["salmon"] = (250, 128, 114),
            ["sandybrown"] = (244, 164, 96),
            ["seagreen"] = (46, 139, 87),
            ["seashell"] = (255, 245, 238),
            ["sienna"] = (160, 82, 45),
            ["silver"] = (192, 192, 192),
            ["skyblue"] = (135, 206, 235),
            ["slateblue"] = (106, 90, 205),
            ["slategray"] = (112, 128, 144),
            ["slategrey"] = (112, 128, 144),
            ["snow"] = (255, 250, 250),
            ["springgreen"] = (0, 255, 127),
            ["steelblue"] = (70, 130, 180),
            ["tan"] = (210, 180, 140),
            ["teal"] = (0, 128, 128),
            ["thistle"] = (216, 191, 216),
            ["tomato"] = (255, 99, 71),
            ["turquoise"] = (64, 224, 208),
            ["violet"] = (238, 130, 238),
            ["wheat"] = (245, 222, 179),
            ["white"] = (255, 255, 255),
            ["whitesmoke"] = (245, 245, 245),
            ["yellow"] = (255, 255, 0),
            ["yellowgreen"] = (154, 205, 50),
        };
}
