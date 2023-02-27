using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("line")]
    [PlotCommandMethod("draw", snippet: "draw(${1:point1_x}, ${2:point1_y},${3:point2_x},${4:point2_y});")]
    class LineCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            switch (Method?.ToLower())
            {
                case "draw":
                    var points = parameters.ToPoints().Take(2).ToArray();
                    if (points.Length == 2)
                    {
                        using (var pen = context.CreatePen(parameters.Skip(4)))
                        {
                            context.Canvas.DrawLine(pen,
                                context.Project(points[0]),
                                context.Project(points[1]));
                        }
                    }
                    break;
                default:
                    throw new Exception($"Unknown method: { Method  }");
            }
        }
    }
}
