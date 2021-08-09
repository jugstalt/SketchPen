using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("line")]
    class LineCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            switch(Method?.ToLower())
            {
                case "draw":
                    var points = parameters.ToPoints().Take(2).ToArray();
                    if (points.Length == 2)
                    {
                        using (var pen = context.CreatePen())
                            context.GraphicsContext.DrawLine(pen.Pen,
                                context.Project(points[0]),
                                context.Project(points[1]));
                    }
                    break;
            }
        }
    }
}
