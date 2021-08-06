using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Reflection
{
    public class PlotCommandKeywordAttribute : Attribute
    {
        public PlotCommandKeywordAttribute(string keyword)
        {
            this.Keyword = keyword;
        }

        public string Keyword { get; }
    }
}
