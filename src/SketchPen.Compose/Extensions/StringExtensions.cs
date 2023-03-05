using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Compose.Extensions;

static internal class StringExtensions
{
    static public string OrTake(this string input, string output)
    {
        if (string.IsNullOrEmpty(input))
        {
            return output;
        }

        return input;
    }
}
