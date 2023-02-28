using System;

namespace SketchPen.Plot.Reflection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PlotCommandKeywordAttribute : Attribute
{
    public PlotCommandKeywordAttribute(string keyword)
    {
        this.Keyword = keyword;
    }

    public string Keyword { get; }
}
