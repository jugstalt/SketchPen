using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands;

[PlotCommandKeyword("gradientbrush")]
[PlotCommandSupportedFileTypes(EditorFileType.Globals | EditorFileType.Template | EditorFileType.Code)]
[PlotCommandMethod("color", snippet: "color(\"${1:hexColor}\");")]
[PlotCommandMethod("points", snippet: "points(${1:point1_x}, ${2:point1_y},${3:point2_x},${4:point2_y});")]
class GradientBrushCommand : GeneralPlotCommand
{
    protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
    {
        switch (Method?.ToLower())
        {
            case "color":
                context.GradientBrushColor = parameters.ToColor();
                break;
            case "points":
                context.GradientBrushPoint1 = parameters.ToPoints().FirstOrDefault();
                context.GradientBrushPoint2 = parameters.ToPoints().Skip(2).FirstOrDefault();
                break;
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
