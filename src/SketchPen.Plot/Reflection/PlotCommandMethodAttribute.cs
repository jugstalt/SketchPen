using System;

namespace SketchPen.Plot.Reflection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PlotCommandMethodAttribute : Attribute
    {
        public PlotCommandMethodAttribute(string name,
                                          string label = "",
                                          string insertText = "")
        {
            Name = name;
            Label = label;
            InsertText = insertText;
        }

        public string Name { get; }
        public string Label { get; }
        public string InsertText { get; }
    }
}
