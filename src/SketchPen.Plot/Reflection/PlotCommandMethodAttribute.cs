using System;

namespace SketchPen.Plot.Reflection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PlotCommandMethodAttribute : Attribute
{
    public PlotCommandMethodAttribute(string name,
                                      string suggestion = "",
                                      string snippet = "")
    {
        Name = name;
        Suggestion = suggestion;
        Snippet = snippet;
    }

    public string Name { get; }
    public string Suggestion { get; }
    public string Snippet { get; }
}
