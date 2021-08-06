using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Platform
{
    static public class NumberConverter
    {
        static public double ToDouble(this string value)
        {
            if (PlatformInfo.IsWindows)
                return double.Parse(value.Replace(",", "."), PlatformInfo.Nhi);

            return double.Parse(value.Replace(",", PlatformInfo.Cnf.NumberDecimalSeparator));
        }

        static public float ToFloat(this string value)
        {
            if (PlatformInfo.IsWindows)
                return float.Parse(value.Replace(",", "."), PlatformInfo.Nhi);

            return float.Parse(value.Replace(",", PlatformInfo.Cnf.NumberDecimalSeparator));
        }

        static public string ToDoubleString(this double d)
        {
            return d.ToString(PlatformInfo.Nhi);
        }
    }
}
