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
                // ToPoints() already groups the 4 raw floats into 2 CanvasPoints -- Skip(1) here
                // skips the first *point* to reach the second one. The previous Skip(2) skipped
                // both points (there are only 2), so GradientBrushPoint2 silently became
                // default(CanvasPoint) (0,0) regardless of the x2,y2 actually given.
                // The points are given in logical units like every other coordinate, so they are
                // projected to pixels here -- the context's own defaults (the canvas corners) are
                // already pixels. Without this the gradient axis depended on the render size.
                var points = parameters.ToPoints();
                context.GradientBrushPoint1 = context.Project(points.FirstOrDefault());
                context.GradientBrushPoint2 = context.Project(points.Skip(1).FirstOrDefault());
                break;
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
