using System;

namespace SketchPen.Plot.Reflection;

public class PlotCommandSupportedFileTypesAttribute : Attribute
{
    public PlotCommandSupportedFileTypesAttribute(EditorFileType editorFileTypes)
    {
        EditorFileTypes = editorFileTypes;
    }

    public EditorFileType EditorFileTypes { get; }
}
