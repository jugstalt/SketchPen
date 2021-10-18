using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Extensions
{
    static class StringExtensions
    {
        static public void AppendCodefileComment(this StringBuilder code, string codeFileName)
        {
            code.Append($"{ Environment.NewLine }// Codefile: { codeFileName };{ Environment.NewLine }");
        }

        static public bool IsCodefileComment(this string codeline)
        {
            return (codeline.StartsWith("// CodeFile: ", StringComparison.InvariantCultureIgnoreCase));
        }

        static public string GetCodefile(this string codeline)
        {
            if (!codeline.IsCodefileComment())
                return null;

            return codeline.Substring("// CodeFile: ".Length);
        }
    }
}
