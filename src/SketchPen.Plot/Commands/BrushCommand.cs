using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Commands;

[PlotCommandKeyword("brush")]
[PlotCommandSupportedFileTypes(EditorFileType.Globals | EditorFileType.Template | EditorFileType.Code)]
[PlotCommandMethod("color", snippet: "color(\"${1:hexColor}\");")]
class BrushCommand : GeneralPlotCommand
{
    protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
    {
        switch (Method?.ToLower())
        {
            case "color":
                context.BrushColor = parameters.ToColor();
                break;
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
