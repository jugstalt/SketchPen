using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Extensions
{
    static class StringExtensions
    {
        static public void AppendCodefileComment(this StringBuilder code, string codeFileName)
        {
            code.Append($"// Codefile: { codeFileName }{ Environment.NewLine }");
        }
    }
}
