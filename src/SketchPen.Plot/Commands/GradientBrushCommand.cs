using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("gradientbrush")]
    class GradientBrushCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            switch(Method?.ToLower())
            {
                case "color":
                    context.GradientBrushColor = parameters.ToColor();
                    break;
                case "points":
                    context.GradientBrushPoint1 = parameters.ToPoints().FirstOrDefault();
                    context.GradientBrushPoint2 = parameters.ToPoints().Skip(1).FirstOrDefault();
                    break;
            }
        }
    }
}
