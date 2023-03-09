using System;
using System.Globalization;

namespace SketchPen.Compose.Extensions;

static internal class NumberExtensions
{
    public static string ToInvariantString(this float number)
        => number.ToString(CultureInfo.InvariantCulture);

    public static string ToInvariantString(this double number)
        => number.ToString(CultureInfo.InvariantCulture);

    public static string ToResolutionsFolder(this float dpi)
        => Math.Round(dpi / 96f, 1).ToInvariantString();
}
